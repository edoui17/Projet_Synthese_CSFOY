import re

# 1. StateMachine.cs (HasState, GetState) - let's make sure they are in the class.
with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'r') as f:
    content = f.read()

if "public bool HasState" not in content:
    replacement = """    public bool HasState(Godot.StringName stateName) => m_states.ContainsKey(stateName);

    public State GetState(Godot.StringName stateName) => m_states.TryGetValue(stateName, out var state) ? state : null;

    public void ForceTransition(StringName p_targetStateName)"""
    content = re.sub(r'    public void ForceTransition\(StringName p_targetStateName\)', replacement, content)
    with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'w') as f:
        f.write(content)

# 2. ChaseState.cs, IdleState.cs, RepositionState.cs access StateMachine.AttackRange, etc.
# Need to replace StateMachine.AttackRange with ((AggressiveNpcBase)NpcContext).AttackRange
def patch_state(file_path):
    with open(file_path, 'r') as f:
        state_content = f.read()

    state_content = state_content.replace('StateMachine.MaxAttackRange', '((IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase)NpcContext).MaxAttackRange')
    state_content = state_content.replace('StateMachine.MinAttackRange', '((IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase)NpcContext).MinAttackRange')
    state_content = state_content.replace('StateMachine.AttackRange', '((IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase)NpcContext).AttackRange')

    with open(file_path, 'w') as f:
        f.write(state_content)

patch_state('./Src/IslandSurvivor/Logic/StateMachine/States/ChaseState.cs')
patch_state('./Src/IslandSurvivor/Logic/StateMachine/States/IdleState.cs')
patch_state('./Src/IslandSurvivor/Logic/StateMachine/States/RepositionState.cs')

# 3. StateMachine.cs (NpcContext.GetDecisionState)
# NpcContext is a CharacterBody2D. Need to cast to NpcBase or INpc, but INpc doesn't have GetDecisionState. Let's cast to NpcBase.
with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'r') as f:
    content = f.read()
content = content.replace('NpcContext.GetDecisionState(target);', '((IslandSurvivor.Scenes.NPC.NpcBase)NpcContext).GetDecisionState(target);')
with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'w') as f:
    f.write(content)
