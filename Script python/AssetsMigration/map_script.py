import os
import shutil

free_pack = "./Src/IslandSurvivor/Assets/TinySwords(FreePack)"
update_pack = "./Src/IslandSurvivor/Assets/TinySwords(Update010)"
target_base = "./Src/IslandSurvivor/Assets/TinySwords"

def get_mapping(base_dir, path):
    rel_path = os.path.relpath(path, base_dir)
    parts = rel_path.split(os.sep)

    if len(parts) < 2:
        return None, None

    top_level = parts[0]
    filename = parts[-1]

    # Check if it's a file
    if not os.path.isfile(path):
        return None, None

    category = None
    if top_level in ["Factions", "Units"]:
        category = "Sprites"
    elif top_level == "Terrain":
        category = "Terrain"
    elif top_level == "UI":
        category = "UI"
    elif top_level in ["Deco", "Resources"]:
        category = "Decorations"
    elif top_level == "Effects":
        category = "Effects"
    elif top_level == "Buildings": # Assuming this maps to Decorations
        category = "Decorations"
    else:
        category = "Misc"

    target_dir = ""

    if category == "Sprites":
        class_name = None
        color_name = None

        if top_level == "Units": # FreePack
            if len(parts) >= 4 and " Units" in parts[1]:
                color_name = parts[1].replace(" Units", "").strip()
                class_name = parts[2]
            elif len(parts) >= 3 and parts[-2] == "Units (aseprite in Blue only)":
                class_name = filename.split(".")[0]
                color_name = "Blue"
            else:
                class_name = parts[-2]

        elif top_level == "Factions": # Update010
            if "Troops" in parts:
                troop_idx = parts.index("Troops")
                if troop_idx + 1 < len(parts) - 1:
                    class_name = parts[troop_idx + 1]
                    colors = ["Blue", "Purple", "Red", "Yellow"]
                    # Usually next is color
                    if parts[troop_idx + 2] in colors:
                        color_name = parts[troop_idx + 2]
                    elif len(parts) > troop_idx + 3 and parts[troop_idx + 3] in colors:
                        color_name = parts[troop_idx + 3]
            elif "Dead" in parts:
                class_name = "Dead"

            if not class_name:
                class_name = parts[-2]

        if class_name and color_name:
            target_dir = f"{category}/{class_name}/{color_name}"
        elif class_name:
            target_dir = f"{category}/{class_name}"
        else:
            target_dir = f"{category}/{parts[-2]}"

    else:
        # User Rule 1: Move final leaf folder directly into category
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

# Group by destination directory
dest_map = {}
for base, path, target_dir, filename in mappings:
    dest_path = os.path.join(target_base, target_dir, filename)
    dest_dir_full = os.path.join(target_base, target_dir)

    if dest_dir_full not in dest_map:
        dest_map[dest_dir_full] = []

    dest_map[dest_dir_full].append({
        "source_base": base,
        "source_path": path,
        "filename": filename
    })

for d in dest_map:
    print(f"DIR: {d} HAS {len(dest_map[d])} FILES")
