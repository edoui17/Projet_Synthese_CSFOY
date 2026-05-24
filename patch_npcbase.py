import re

with open('./Src/IslandSurvivor/Scenes/NPC/NpcBase.cs', 'r') as f:
    content = f.read()

replacement = """    public virtual string CurrentState => m_stateMachine?.CurrentState?.Name ?? "";

    public virtual Godot.StringName GetDecisionState(Node2D target)
    {
        return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
    }"""

content = re.sub(r'    public virtual string CurrentState => m_stateMachine\?\.CurrentState\?\.Name \?\? "";', replacement, content)

with open('./Src/IslandSurvivor/Scenes/NPC/NpcBase.cs', 'w') as f:
    f.write(content)
