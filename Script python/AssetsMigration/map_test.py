import os

free_pack = "./Src/IslandSurvivor/Assets/TinySwords(FreePack)"
update_pack = "./Src/IslandSurvivor/Assets/TinySwords(Update010)"

def map_path(base_dir, path):
    rel_path = os.path.relpath(path, base_dir)
    parts = rel_path.split(os.sep)

    if len(parts) < 2:
        return None

    top_level = parts[0]
    filename = parts[-1]

    # Check if it's a file
    if not os.path.isfile(path):
        return None

    # We want to identify the category and the subfolders
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
    else:
        # Default fallback (e.g., Buildings in FreePack wasn't mentioned but let's assume it goes to Decorations, or maybe we just use Misc for now to see)
        # Actually user said: "[Leaf Folders from Deco/Resources/Effects] -> `TinySwords/Decorations/[FolderName]/`"
        # Wait, the user said: "Deco/ and Resources/ -> TinySwords/Decorations/"
        # Let's map Buildings to Decorations too if there's no other rule. I'll ask or assume Misc.
        # Actually, let's just map everything not specified to its own category to be safe, or Misc.
        # Wait, the prompt didn't mention Buildings. I'll map them to `Decorations/{leaf}` to be safe.
        if top_level == "Buildings":
            category = "Decorations"
        else:
            category = "Misc"

    # User rule 1: Flatten hierarchies. Move the final leaf folder directly into the category.
    # User rule 2: Use explicit mapping for merge.
    # User rule 3 (clarification): "use the parent category and keep the color division inside with subfolder per color" for units.

    target_dir = ""

    if category == "Sprites":
        # FreePack: Units/Blue Units/Warrior/Warrior_Attack1.png -> parts = ["Units", "Blue Units", "Warrior", "Warrior_Attack1.png"]
        # Update010: Factions/Knights/Troops/Warrior/Blue/Warrior_Blue.png -> parts = ["Factions", "Knights", "Troops", "Warrior", "Blue", "Warrior_Blue.png"]

        # We need to extract Class and Color. If no color, just Class.
        # If it's something else, just use the leaf folder.

        class_name = None
        color_name = None

        if top_level == "Units":
            # FreePack
            if len(parts) == 4 and " Units" in parts[1]:
                color_name = parts[1].replace(" Units", "").strip()
                class_name = parts[2]
            elif len(parts) >= 3:
                # E.g. Units/Units (aseprite in Blue only)/Archer.aseprite
                if parts[-2] == "Units (aseprite in Blue only)":
                    class_name = filename.split(".")[0] # E.g., Archer
                    color_name = "Blue"
                else:
                    class_name = parts[-2]

        elif top_level == "Factions":
            # Update010
            # Path usually: Factions/Knights/Troops/Warrior/Blue/Warrior_Blue.png
            # Or: Factions/Knights/Troops/Archer/Archer + Bow/Archer_Blue.png
            if "Troops" in parts:
                troop_idx = parts.index("Troops")
                if troop_idx + 1 < len(parts) - 1:
                    class_name = parts[troop_idx + 1]
                    # Check if next part is a color
                    next_part = parts[troop_idx + 2]
                    colors = ["Blue", "Purple", "Red", "Yellow"]
                    if next_part in colors:
                        color_name = next_part
                    elif len(parts) > troop_idx + 3 and parts[troop_idx + 3] in colors: # Might not happen
                        color_name = parts[troop_idx + 3]
            elif "Dead" in parts:
                class_name = "Dead"

            # If no class found, just use leaf folder
            if not class_name:
                class_name = parts[-2]

        if class_name and color_name:
            target_dir = f"TinySwords/{category}/{class_name}/{color_name}"
        elif class_name:
            target_dir = f"TinySwords/{category}/{class_name}"
        else:
            target_dir = f"TinySwords/{category}/{parts[-2]}"

    else:
        # All other categories: Leaf folder directly inside the category
        leaf_folder = parts[-2]
        target_dir = f"TinySwords/{category}/{leaf_folder}"

    return target_dir, filename

for base in [free_pack, update_pack]:
    if not os.path.exists(base): continue
    for root, dirs, files in os.walk(base):
        for file in files:
            path = os.path.join(root, file)
            mapped = map_path(base, path)
            if mapped:
                print(f"{os.path.relpath(path, base)} -> {mapped[0]}/{mapped[1]}")
