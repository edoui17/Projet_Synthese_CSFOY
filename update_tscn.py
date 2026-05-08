import os
import re

def update_tscn(filepath, stat_manager_id, entity_type_value):
    with open(filepath, 'r') as f:
        content = f.read()

    # Find the StatManager node instance. It looks like:
    # [node name="StatManager" parent="." unique_id=... instance=ExtResource("...")]

    # We will simply look for "name=\"StatManager\"" or "name=\"StatsManager\"" inside a [node ...] block.
    # We need to insert `m_entityType = X` after the `[node ...]` line

    # Alternatively we can look for `instance=ExtResource("7_stat")` but the id might change.

    pattern = re.compile(r'(\[node name="Stat(s?)Manager".*?\])(.*?)(\n\n|\[node)', re.DOTALL)

    def repl(match):
        node_def = match.group(1)
        node_props = match.group(3)
        end = match.group(4)

        # Check if m_entityType already exists
        if "m_entityType =" in node_props:
            node_props = re.sub(r'm_entityType = \d+', f'm_entityType = {entity_type_value}', node_props)
        else:
            if not node_props.endswith("\n"):
                node_props += "\n"
            node_props += f"m_entityType = {entity_type_value}\n"

        return node_def + node_props + end

    new_content = pattern.sub(repl, content)

    # Try another pattern if it's the root node StatsManager
    pattern_root = re.compile(r'(\[node name="StatsManager" type="Node2D".*?\])(.*?)(\n\n|$)', re.DOTALL)

    def repl_root(match):
        node_def = match.group(1)
        node_props = match.group(2)
        end = match.group(3)

        if "m_entityType =" in node_props:
            node_props = re.sub(r'm_entityType = \d+', f'm_entityType = {entity_type_value}', node_props)
        else:
            if not node_props.endswith("\n"):
                node_props += "\n"
            node_props += f"m_entityType = {entity_type_value}\n"

        return node_def + node_props + end

    if "StatManager" not in new_content and "StatsManager" in new_content:
        new_content = pattern_root.sub(repl_root, new_content)

    with open(filepath, 'w') as f:
        f.write(new_content)

    print(f"Updated {filepath} to EntityType {entity_type_value}")

files_to_update = [
    ("Src/IslandSurvivor/Scenes/Ressources/Gold/Gold.tscn", 2),
    ("Src/IslandSurvivor/Scenes/Ressources/Rock/Rock.tscn", 2),
    ("Src/IslandSurvivor/Scenes/Ressources/Tree/AutomnTree.tscn", 2),
    ("Src/IslandSurvivor/Scenes/Ressources/Tree/ConiferTree.tscn", 2),
    ("Src/IslandSurvivor/Scenes/Player/Player.tscn", 0),
    ("Src/IslandSurvivor/Scenes/NPC/Agressive/Soldier.tscn", 1),
    ("Src/IslandSurvivor/Scenes/NPC/Passive/Sheep.tscn", 1),
    ("Src/IslandSurvivor/Nodes/StatsManager/StatsManager.tscn", 1) # default
]

for fp, val in files_to_update:
    if os.path.exists(fp):
        update_tscn(fp, "", val)
    else:
        print(f"Not found: {fp}")
