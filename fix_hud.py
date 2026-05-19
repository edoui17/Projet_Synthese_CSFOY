filepath = "./Src/IslandSurvivor/Scenes/HomeMap/PlayerHUD.tscn"

with open(filepath, "r") as f:
    content = f.read()

import_str = """[ext_resource type="PackedScene" uid="uid://derr2icro3xvq" path="res://Scenes/Inventory/HealthBar.tscn" id="2_m0shc"]
[ext_resource type="Script" uid="uid://cwgy214xflfdk" path="res://Nodes/ScoreManager/ScoreUI.cs" id="3_scoreui"]"""

content = content.replace('[ext_resource type="PackedScene" uid="uid://derr2icro3xvq" path="res://Scenes/Inventory/HealthBar.tscn" id="2_m0shc"]', import_str)


node_str = """
[node name="ScoreUI" type="Label" parent="." unique_id=987654322]
layout_mode = 1
anchors_preset = 0
offset_left = 20.0
offset_top = 20.0
offset_right = 150.0
offset_bottom = 60.0
theme_override_font_sizes/font_size = 24
text = "Score: 0"
script = ExtResource("3_scoreui")
"""

content += node_str

with open(filepath, "w") as f:
    f.write(content)
