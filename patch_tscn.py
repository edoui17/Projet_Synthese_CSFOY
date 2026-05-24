import re
import os

# Let's see how many ext_resources are already there and inject states
state_resources = """
[ext_resource type="PackedScene" path="res://Nodes/State/Core/States/IdleState.tscn" id="state_IdleState"]
[ext_resource type="PackedScene" path="res://Nodes/State/Core/States/WanderState.tscn" id="state_WanderState"]
[ext_resource type="PackedScene" path="res://Nodes/State/Core/States/ChaseState.tscn" id="state_ChaseState"]
[ext_resource type="PackedScene" path="res://Nodes/State/Core/States/AttackState.tscn" id="state_AttackState"]
[ext_resource type="PackedScene" path="res://Nodes/State/Core/States/RecoveryState.tscn" id="state_RecoveryState"]
[ext_resource type="PackedScene" path="res://Nodes/State/Core/States/WindUpState.tscn" id="state_WindUpState"]
[ext_resource type="PackedScene" path="res://Nodes/State/Core/States/DeathState.tscn" id="state_DeathState"]
"""

state_nodes = """
[node name="StateMachine" parent="." index="7"]
InitialState = NodePath("IdleState")

[node name="IdleState" parent="StateMachine" index="0" instance=ExtResource("state_IdleState")]

[node name="WanderState" parent="StateMachine" index="1" instance=ExtResource("state_WanderState")]

[node name="ChaseState" parent="StateMachine" index="2" instance=ExtResource("state_ChaseState")]

[node name="WindUpState" parent="StateMachine" index="3" instance=ExtResource("state_WindUpState")]

[node name="AttackState" parent="StateMachine" index="4" instance=ExtResource("state_AttackState")]

[node name="RecoveryState" parent="StateMachine" index="5" instance=ExtResource("state_RecoveryState")]

[node name="DeathState" parent="StateMachine" index="6" instance=ExtResource("state_DeathState")]
"""

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.tscn', 'r') as f:
    content = f.read()

# Add ext_resources
if "state_IdleState" not in content:
    idx = content.find('\n[sub_resource')
    if idx != -1:
        content = content[:idx] + state_resources + content[idx:]
    else:
        content = content + state_resources

# Add nodes
if "StateMachine" not in content:
    content = content + state_nodes

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.tscn', 'w') as f:
    f.write(content)

# Clean up Lancer.tscn
with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Lancer/Lancer.tscn', 'r') as f:
    lancer_content = f.read()

# Remove state instances that are now in AggressiveNpcBase
lancer_content = re.sub(r'\[node name="IdleState".*?\[node name="WanderState"', '[node name="WanderState"', lancer_content, flags=re.DOTALL)
lancer_content = re.sub(r'\[node name="WanderState".*?\[node name="ChaseState"', '[node name="ChaseState"', lancer_content, flags=re.DOTALL)
lancer_content = re.sub(r'\[node name="ChaseState".*?\[node name="WindUpState"', '[node name="WindUpState"', lancer_content, flags=re.DOTALL)
lancer_content = re.sub(r'\[node name="WindUpState".*?\[node name="DashState"', '[node name="DashState"', lancer_content, flags=re.DOTALL)
lancer_content = re.sub(r'\[node name="RecoveryState".*?\[node name="AttackState"', '[node name="AttackState"', lancer_content, flags=re.DOTALL)
lancer_content = re.sub(r'\[node name="AttackState".*?\[node name="LancerRepositionState"', '[node name="LancerRepositionState"', lancer_content, flags=re.DOTALL)
lancer_content = re.sub(r'\[node name="DeathState".*?$', '', lancer_content, flags=re.DOTALL)

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Lancer/Lancer.tscn', 'w') as f:
    f.write(lancer_content)
