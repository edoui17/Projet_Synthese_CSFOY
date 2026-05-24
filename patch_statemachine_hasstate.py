import re

with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'r') as f:
    content = f.read()

replacement = """    public bool HasState(Godot.StringName stateName) => m_states.ContainsKey(stateName);

    public State GetState(Godot.StringName stateName) => m_states.TryGetValue(stateName, out var state) ? state : null;

    public void ForceTransition(StringName p_targetStateName)"""

content = re.sub(r'    public void ForceTransition\(StringName p_targetStateName\)', replacement, content)

with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'w') as f:
    f.write(content)
