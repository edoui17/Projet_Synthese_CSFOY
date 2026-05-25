import re

files_to_patch = [
    'Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Soldier/Soldier.tscn',
    'Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Lancer/Lancer.tscn',
    'Src/IslandSurvivor/Scenes/TestScenes/ReaperTestScene.tscn' # if any
]

for file_path in files_to_patch:
    try:
        with open(file_path, 'r') as f:
            content = f.read()

        # Replace AttackState with MeleeAttackState
        content = re.sub(
            r'\[ext_resource type="PackedScene" uid="([^"]+)" path="res://Nodes/State/Combat/AttackState\.tscn" id="state_AttackState"\]',
            '[ext_resource type="PackedScene" uid="uid://cx6r1g1g1g1g1" path="res://Nodes/State/Combat/MeleeAttackState.tscn" id="state_MeleeAttackState"]',
            content
        )
        content = re.sub(r'\[node name="AttackState" parent="StateMachine"([^\]]*)instance=ExtResource\("state_AttackState"\)\]',
                         r'[node name="MeleeAttackState" parent="StateMachine"\1instance=ExtResource("state_MeleeAttackState")]', content)

        with open(file_path, 'w') as f:
            f.write(content)
    except Exception as e:
        print(f"Skipping {file_path}")
