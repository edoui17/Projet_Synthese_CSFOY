import re

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Lancer/Lancer.tscn', 'r') as f:
    content = f.read()

# Remove instance=ExtResource(...) from the states that are inherited
content = re.sub(r'\[node name="IdleState" parent="StateMachine" .*? instance=ExtResource\("state_IdleState"\)\]', '[node name="IdleState" parent="StateMachine" index="0"]', content)
content = re.sub(r'\[node name="WanderState" parent="StateMachine" instance=ExtResource\("state_WanderState"\)\]', '[node name="WanderState" parent="StateMachine" index="1"]', content)
content = re.sub(r'\[node name="ChaseState" parent="StateMachine" .*? instance=ExtResource\("state_ChaseState"\)\]', '[node name="ChaseState" parent="StateMachine" index="2"]', content)
content = re.sub(r'\[node name="WindUpState" parent="StateMachine" .*? instance=ExtResource\("state_WindUpState"\)\]', '[node name="WindUpState" parent="StateMachine" index="3"]', content)
content = re.sub(r'\[node name="RecoveryState" parent="StateMachine" .*? instance=ExtResource\("state_RecoveryState"\)\]', '[node name="RecoveryState" parent="StateMachine" index="5"]', content)
content = re.sub(r'\[node name="AttackState" parent="StateMachine" instance=ExtResource\("state_AttackState"\)\]', '[node name="AttackState" parent="StateMachine" index="4"]', content)
content = re.sub(r'\[node name="DeathState" parent="StateMachine" .*? instance=ExtResource\("state_DeathState"\)\]', '[node name="DeathState" parent="StateMachine" index="6"]', content)

# Remove the state_ExtResource paths entirely from Lancer.tscn as they are no longer needed
content = re.sub(r'\[ext_resource type="PackedScene" path="res://Nodes/State/Core/States/(Idle|Wander|Chase|Attack|Recovery|WindUp|Death)State.tscn" id="state_\1State"\]\n', '', content)

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Lancer/Lancer.tscn', 'w') as f:
    f.write(content)
