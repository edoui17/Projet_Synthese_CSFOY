file_path = 'Src/IslandSurvivor/Scenes/NPC/Aggressive/Boss/Reaper/Reaper.tscn'
with open(file_path, 'r') as f:
    content = f.read()

import re

# Add Shooter Node under Reaper, replace AttackState with MeleeAttackState and RangedAttackState
content = re.sub(
    r'\[ext_resource type="PackedScene" uid="uid://tv17dq11ra3eo" path="res://Nodes/State/Combat/AttackState.tscn" id="state_AttackState"\]',
    '[ext_resource type="PackedScene" uid="uid://cx6r1g1g1g1g1" path="res://Nodes/State/Combat/MeleeAttackState.tscn" id="state_MeleeAttackState"]\n[ext_resource type="PackedScene" uid="uid://cx6r1g1g1g1g2" path="res://Nodes/State/Combat/RangedAttackState.tscn" id="state_RangedAttackState"]\n[ext_resource type="PackedScene" uid="uid://cx6r1g1g1g1g3" path="res://Nodes/Combat/Shooter.tscn" id="Shooter"]',
    content
)

content = re.sub(
    r'\[node name="AttackState" parent="StateMachine" instance=ExtResource\("state_AttackState"\)\]',
    '[node name="MeleeAttackState" parent="StateMachine" instance=ExtResource("state_MeleeAttackState")]\nAttackAnimationName = "MeleeAttackNormal"\n[node name="RangedAttackState" parent="StateMachine" instance=ExtResource("state_RangedAttackState")]\nAttackAnimationName = "ScytheThrow"\n',
    content
)

# Insert Shooter Node before StateMachine
content = re.sub(
    r'\[node name="HitboxAreaLeft" parent="\." index="8" unique_id=[0-9]+\]\nposition = Vector2\(-2, 0\)\n\n\[node name="CollisionPolygon2D" type="CollisionPolygon2D" parent="HitboxAreaLeft" unique_id=[0-9]+\]\nscale = Vector2\(-1, 1\)\npolygon = PackedVector2Array\(45, -96, 93, -61, 113, -13, 116, 44, 98, 74, 45, 74\)',
    '\\g<0>\n\n[node name="Shooter" parent="." instance=ExtResource("Shooter")]\nCooldown = 4.0\nProjectileScene = ExtResource("5_scythe")',
    content
)


with open(file_path, 'w') as f:
    f.write(content)
