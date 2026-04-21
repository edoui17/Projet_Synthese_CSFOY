import re

with open("Src/IslandSurvivor/Scenes/Player/Player.tscn", "r") as f:
    content = f.read()

# Add ext_resource for MovementController
ext_resource_str = '[ext_resource type="PackedScene" path="res://Nodes/Movement/MovementController.tscn" id="9_mov"]\n'
content = content.replace('[ext_resource type="Resource" path="res://Resources/Stats/Entity/PlayerStats.tres" id="8_stat_res"]\n', '[ext_resource type="Resource" path="res://Resources/Stats/Entity/PlayerStats.tres" id="8_stat_res"]\n' + ext_resource_str)

# Add MovementController node
movement_node_str = '''[node name="MovementController" parent="." instance=ExtResource("9_mov")]
Stats = NodePath("../StatManager")

'''
content = content.replace('[node name="Camera2D" type="Camera2D" parent="." unique_id=24708510]\n', '[node name="Camera2D" type="Camera2D" parent="." unique_id=24708510]\n\n' + movement_node_str)

with open("Src/IslandSurvivor/Scenes/Player/Player.tscn", "w") as f:
    f.write(content)
