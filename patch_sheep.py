import re

with open("Src/IslandSurvivor/Scenes/NPC/Passive/Sheep.tscn", "r") as f:
    content = f.read()

# Add ext_resource for MovementController
ext_resource_str = '[ext_resource type="PackedScene" path="res://Nodes/Movement/MovementController.tscn" id="6_mov"]\n'
content = content.replace('[ext_resource type="Resource" uid="uid://baaektliv15bg" path="res://Resources/Stats/Entity/SheepStats.tres" id="5_ifvjj"]\n', '[ext_resource type="Resource" uid="uid://baaektliv15bg" path="res://Resources/Stats/Entity/SheepStats.tres" id="5_ifvjj"]\n' + ext_resource_str)

# Add MovementController node
movement_node_str = '''[node name="MovementController" parent="." instance=ExtResource("6_mov")]
Stats = NodePath("../StatsManager")

'''
content = content.replace('[node name="NavigationAgent2D" type="NavigationAgent2D" parent="." unique_id=1042935962]\n', movement_node_str + '[node name="NavigationAgent2D" type="NavigationAgent2D" parent="." unique_id=1042935962]\n')

with open("Src/IslandSurvivor/Scenes/NPC/Passive/Sheep.tscn", "w") as f:
    f.write(content)
