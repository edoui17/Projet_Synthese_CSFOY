import re
import sys

def process_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    def replace_anim(match):
        full_anim_block = match.group(0)

        # Get length
        len_match = re.search(r'length = ([\d\.]+)', full_anim_block)
        if not len_match: return full_anim_block
        anim_length = len_match.group(1)

        # Count existing tracks
        track_matches = re.findall(r'tracks/(\d+)/', full_anim_block)
        if not track_matches:
            next_track_idx = 0
        else:
            next_track_idx = max([int(x) for x in track_matches]) + 1

        # Check if CancelAttack or EndAttack is already called
        if 'method": &"CancelAttack"' in full_anim_block or 'method": &"EndAttack"' in full_anim_block:
            return full_anim_block

        # Create new track for CancelAttack()
        new_track = f"""tracks/{next_track_idx}/type = "method"
tracks/{next_track_idx}/imported = false
tracks/{next_track_idx}/enabled = true
tracks/{next_track_idx}/path = NodePath("AttackController")
tracks/{next_track_idx}/interp = 1
tracks/{next_track_idx}/loop_wrap = true
tracks/{next_track_idx}/keys = {{
"times": PackedFloat32Array({anim_length}),
"transitions": PackedFloat32Array(1),
"values": [{{
"args": [],
"method": &"CancelAttack"
}}]
}}
"""
        return full_anim_block + new_track

    new_content = re.sub(r'(\[sub_resource type="Animation" id="([^"]+)"\]\nresource_name = "Attack_[^"]+"[\s\S]*?(?=\n\[|$))', replace_anim, content)

    if new_content != content:
        with open(filepath, 'w') as f:
            f.write(new_content)
        print(f"Updated {filepath}")
    else:
        print(f"No changes for {filepath}")

for arg in sys.argv[1:]:
    process_file(arg)
