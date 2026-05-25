import re

file_path = 'Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Archer/Archer.tscn'
with open(file_path, 'r') as f:
    content = f.read()

# Add Shooter Node under Archer, replace AttackState with RangedAttackState
content = re.sub(
    r'\[ext_resource type="PackedScene" uid="([^"]+)" path="res://Nodes/State/Combat/AttackState\.tscn" id="state_AttackState"\]',
    '[ext_resource type="PackedScene" uid="uid://cx6r1g1g1g1g2" path="res://Nodes/State/Combat/RangedAttackState.tscn" id="state_RangedAttackState"]\n[ext_resource type="PackedScene" uid="uid://cx6r1g1g1g1g3" path="res://Nodes/Combat/Shooter.tscn" id="Shooter"]',
    content
)

content = re.sub(
    r'\[node name="AttackState" parent="StateMachine"([^\]]*)instance=ExtResource\("state_AttackState"\)\]',
    '[node name="RangedAttackState" parent="StateMachine"\\1instance=ExtResource("state_RangedAttackState")]\nAttackAnimationName = "Attack"\n',
    content
)

# Insert Shooter Node before StateMachine
content = re.sub(
    r'\[node name="AnimationPlayer" parent="\." index="11" unique_id=[0-9]+\]\nlibraries/ = SubResource\("AnimationLibrary_[A-Za-z0-9]+"\)',
    '\\g<0>\n\n[node name="Shooter" parent="." instance=ExtResource("Shooter")]\nCooldown = 2.0\nProjectileScene = ExtResource("4_l1b6j")',
    content
)

with open(file_path, 'w') as f:
    f.write(content)
