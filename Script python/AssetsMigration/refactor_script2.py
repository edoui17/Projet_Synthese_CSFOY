import os
import json

# Load file map to know where every old file went
with open('file_map.json', 'r') as f:
    file_map = json.load(f)

# Create a mapping of Godot resource paths
godot_path_map = {}
for old_path, new_rel_path in file_map.items():
    old_res_path = old_path.replace("./Src/IslandSurvivor/", "res://")
    old_res_path = old_res_path.replace("\\", "/") # Just in case
    new_res_path = "res://Assets/TinySwords/" + new_rel_path.replace("\\", "/")

    # Exclude import files since godot script path references usually don't end in .import
    # Wait, godot scene files reference .png directly (which maps to uid + path), but sometimes .tres files.
    # We map whatever string might appear in tscn files
    godot_path_map[old_res_path] = new_res_path

godot_dir = "./Src/IslandSurvivor"
refactored_count = 0

sorted_old_paths = sorted(godot_path_map.keys(), key=len, reverse=True)

for root, dirs, files in os.walk(godot_dir):
    if "TinySwords(FreePack)" in root or "TinySwords(Update010)" in root:
        continue

    for file in files:
        if file.endswith(('.tscn', '.tres', '.cs')):
            path = os.path.join(root, file)
            try:
                with open(path, 'r', encoding='utf-8') as f:
                    content = f.read()

                original_content = content

                for old_res in sorted_old_paths:
                    if old_res in content:
                        content = content.replace(old_res, godot_path_map[old_res])

                if content != original_content:
                    with open(path, 'w', encoding='utf-8') as f:
                        f.write(content)
                    refactored_count += 1
            except Exception as e:
                pass

print(f"Total files refactored: {refactored_count}")
