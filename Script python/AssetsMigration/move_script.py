import os
import shutil
import json

free_pack = "./Src/IslandSurvivor/Assets/TinySwords(FreePack)"
update_pack = "./Src/IslandSurvivor/Assets/TinySwords(Update010)"
target_base = "./Src/IslandSurvivor/Assets/TinySwords"

def get_mapping(base_dir, path):
    rel_path = os.path.relpath(path, base_dir)
    parts = rel_path.split(os.sep)
    if len(parts) < 2: return None, None

    top_level = parts[0]
    filename = parts[-1]
    if not os.path.isfile(path): return None, None

    category = None
    if top_level in ["Factions", "Units"]: category = "Sprites"
    elif top_level == "Terrain": category = "Terrain"
    elif top_level == "UI": category = "UI"
    elif top_level in ["Deco", "Resources"]: category = "Decorations"
    elif top_level == "Effects": category = "Effects"
    elif top_level == "Buildings": category = "Buildings" # Added Buildings category
    else: category = "Misc"

    target_dir = ""
    if category == "Sprites":
        class_name = None
        color_name = None

        if top_level == "Units":
            if len(parts) >= 4 and " Units" in parts[1]:
                color_name = parts[1].replace(" Units", "").strip()
                class_name = parts[2]
            elif len(parts) >= 3 and parts[-2] == "Units (aseprite in Blue only)":
                class_name = filename.split(".")[0]
                color_name = "Blue"
            else:
                class_name = parts[-2]

        elif top_level == "Factions":
            if "Troops" in parts:
                troop_idx = parts.index("Troops")
                if troop_idx + 1 < len(parts) - 1:
                    class_name = parts[troop_idx + 1]
                    colors = ["Blue", "Purple", "Red", "Yellow"]
                    if troop_idx + 2 < len(parts) - 1 and parts[troop_idx + 2] in colors:
                        color_name = parts[troop_idx + 2]
                    elif troop_idx + 3 < len(parts) - 1 and parts[troop_idx + 3] in colors:
                        color_name = parts[troop_idx + 3]
            elif "Dead" in parts:
                class_name = "Dead"

            if not class_name: class_name = parts[-2]

        if class_name and color_name:
            target_dir = f"{category}/{class_name}/{color_name}"
        elif class_name:
            target_dir = f"{category}/{class_name}"
        else:
            target_dir = f"{category}/{parts[-2]}"

    else:
        leaf_folder = parts[-2]
        target_dir = f"{category}/{leaf_folder}"

    return target_dir, filename

mappings = []
for base in [free_pack, update_pack]:
    if not os.path.exists(base): continue
    for root, dirs, files in os.walk(base):
        for file in files:
            path = os.path.join(root, file)
            target_dir, filename = get_mapping(base, path)
            if target_dir and filename:
                mappings.append((base, path, target_dir, filename))

os.makedirs(target_base, exist_ok=True)

import_files = [m for m in mappings if m[3].endswith('.import')]
regular_files = [m for m in mappings if not m[3].endswith('.import')]

renamed_files_log = []
rename_map = {}
file_map = {} # Track the final location of everything

def process_file(m, is_import=False):
    base, path, target_dir, filename = m
    dest_dir_full = os.path.join(target_base, target_dir)
    os.makedirs(dest_dir_full, exist_ok=True)

    dest_filename = filename
    original_asset_filename = filename[:-7] if is_import else filename

    dest_path = os.path.join(dest_dir_full, filename)

    if base == update_pack:
        if is_import:
            asset_old_path = path[:-7]
            if asset_old_path in rename_map:
                new_asset_dest_path = rename_map[asset_old_path]
                dest_filename = os.path.basename(new_asset_dest_path) + ".import"
                dest_path = os.path.join(dest_dir_full, dest_filename)
        else:
            if os.path.exists(dest_path):
                name, ext = os.path.splitext(filename)
                dest_filename = f"{name}_v2{ext}"
                dest_path = os.path.join(dest_dir_full, dest_filename)
                rename_map[path] = dest_path
                renamed_files_log.append(f"{os.path.relpath(path, update_pack)} -> {target_dir}/{dest_filename}")

    shutil.copy2(path, dest_path)
    file_map[path] = dest_path

    if is_import and base == update_pack and path[:-7] in rename_map:
        new_asset_filename = os.path.basename(rename_map[path[:-7]])
        with open(dest_path, 'r', encoding='utf-8') as f:
            content = f.read()

        content = content.replace(original_asset_filename, new_asset_filename)

        with open(dest_path, 'w', encoding='utf-8') as f:
            f.write(content)

for m in regular_files:
    process_file(m, is_import=False)

for m in import_files:
    process_file(m, is_import=True)

print(f"Total renamed files to _v2: {len(renamed_files_log)}")
print("\n--- Renamed Files ---")
for r in renamed_files_log:
    print(r)

with open('rename_map.json', 'w') as f:
    json.dump({k: os.path.relpath(v, target_base) for k,v in rename_map.items()}, f, indent=2)

with open('file_map.json', 'w') as f:
    json.dump({k: os.path.relpath(v, target_base) for k,v in file_map.items()}, f, indent=2)
