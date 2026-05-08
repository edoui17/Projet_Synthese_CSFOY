with open("Src/IslandSurvivor/Nodes/StatsManager/StatsManager.tscn", "r") as f:
    content = f.read()

import re
pattern_root = re.compile(r'(\[node name="StatsManager" type="Node2D".*?\])(.*?)(\n\n|$)', re.DOTALL)
def repl_root(match):
    node_def = match.group(1)
    node_props = match.group(2)
    end = match.group(3)

    if "m_entityType =" in node_props:
        node_props = re.sub(r'm_entityType = \d+', f'm_entityType = 1', node_props)
    else:
        if not node_props.endswith("\n"):
            node_props += "\n"
        node_props += f"m_entityType = 1\n"

    return node_def + node_props + end

new_content = pattern_root.sub(repl_root, content)
with open("Src/IslandSurvivor/Nodes/StatsManager/StatsManager.tscn", "w") as f:
    f.write(new_content)
