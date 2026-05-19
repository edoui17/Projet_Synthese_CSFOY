filepath = "./Src/IslandSurvivor/Scenes/Level/LevelBase.tscn"

with open(filepath, "r") as f:
    content = f.read()

import_str = """[ext_resource type="PackedScene" uid="uid://du6mrtfohbxi8" path="res://Scenes/GameMaterialsPlanner/MaterialsMenuPlanner.tscn" id="5_mat"]
[ext_resource type="PackedScene" uid="uid://cxab32" path="res://Scenes/MainMenu/DefeatMenu/DefeatMenu.tscn" id="6_defeat"]"""

content = content.replace('[ext_resource type="PackedScene" uid="uid://du6mrtfohbxi8" path="res://Scenes/GameMaterialsPlanner/MaterialsMenuPlanner.tscn" id="5_mat"]', import_str)

node_str = """size_flags_horizontal = 4
size_flags_vertical = 4

[node name="DefeatMenu" parent="CanvasLayer" unique_id=987654321 instance=ExtResource("6_defeat")]
"""

# Only replace the LAST occurrence to avoid double inserts
idx = content.rfind("""size_flags_horizontal = 4
size_flags_vertical = 4
""")

if idx != -1:
    content = content[:idx] + node_str + content[idx + len("""size_flags_horizontal = 4\nsize_flags_vertical = 4\n"""):]

with open(filepath, "w") as f:
    f.write(content)
