import os
import json

# Load file map to know where every old file went
with open('file_map.json', 'r') as f:
    file_map = json.load(f)

# Create a mapping of Godot resource paths
godot_path_map = {}
for old_path, new_rel_path in file_map.items():
    # old_path looks like "./Src/IslandSurvivor/Assets/TinySwords(FreePack)/..."
    # We want to convert this to Godot paths
    # Assuming "Src/IslandSurvivor" is the root of the Godot project (res://)
    # Let's verify where project.godot is
    # Actually, usually project.godot is in Src/IslandSurvivor

    # Old path format: ./Src/IslandSurvivor/Assets/...
    # Convert to res://Assets/...

    old_res_path = old_path.replace("./Src/IslandSurvivor/", "res://")
    old_res_path = old_res_path.replace("\\", "/") # Just in case

    # new_rel_path is relative to target_base which is ./Src/IslandSurvivor/Assets/TinySwords
    # new_res_path should be res://Assets/TinySwords/ + new_rel_path
    new_res_path = "res://Assets/TinySwords/" + new_rel_path.replace("\\", "/")

    godot_path_map[old_res_path] = new_res_path

# We also need to be careful about uid paths? Godot uses both uid://... and res://...
# If the file hasn't changed its uid, we only need to update the res:// paths.
# The user rule states: Update all `res://` paths to the new structure.

godot_dir = "./Src/IslandSurvivor"

refactored_count = 0

for root, dirs, files in os.walk(godot_dir):
    # Skip the actual old asset folders to speed up, though we'll delete them later anyway
    if "TinySwords(FreePack)" in root or "TinySwords(Update010)" in root:
        continue

    for file in files:
        if file.endswith(('.tscn', '.tres', '.cs')):
            path = os.path.join(root, file)
            with open(path, 'r', encoding='utf-8') as f:
                content = f.read()

            original_content = content

            # Since godot_path_map has ~1700 entries, a simple iterate and replace might be fine
            # We sort by length descending to replace more specific paths first, just in case
            sorted_old_paths = sorted(godot_path_map.keys(), key=len, reverse=True)

            for old_res in sorted_old_paths:
                if old_res in content:
                    content = content.replace(old_res, godot_path_map[old_res])

            if content != original_content:
                with open(path, 'w', encoding='utf-8') as f:
                    f.write(content)
                refactored_count += 1
                # print(f"Updated {path}")

print(f"Total files refactored: {refactored_count}")
