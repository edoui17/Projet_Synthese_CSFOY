filepath = "./Src/IslandSurvivor/Scenes/Level/LevelBase.tscn"

with open(filepath, "r") as f:
    content = f.read()

import_str = """[ext_resource type="PackedScene" uid="uid://du6mrtfohbxi8" path="res://Scenes/GameMaterialsPlanner/MaterialsMenuPlanner.tscn" id="5_mat"]
[ext_resource type="PackedScene" uid="uid://cxab32" path="res://Scenes/MainMenu/DefeatMenu/DefeatMenu.tscn" id="6_defeat"]"""

content = content.replace('[ext_resource type="PackedScene" uid="uid://du6mrtfohbxi8" path="res://Scenes/GameMaterialsPlanner/MaterialsMenuPlanner.tscn" id="5_mat"]', import_str)

node_str = """size_flags_horizontal = 4
size_flags_vertical = 4

[node name="DefeatMenu" parent="." unique_id=987654321 instance=ExtResource("6_defeat")]
"""

content = content.replace("""size_flags_horizontal = 4
size_flags_vertical = 4
""", node_str)

with open(filepath, "w") as f:
    f.write(content)

print("Updated LevelBase.tscn")
