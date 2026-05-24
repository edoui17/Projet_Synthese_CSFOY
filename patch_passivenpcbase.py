import re

with open('./Src/IslandSurvivor/Scenes/NPC/Passive/PassiveNpcBase.cs', 'r') as f:
    content = f.read()

replacement = """    public override string CurrentState => m_passiveController?.CurrentState ?? NpcStates.IDLE;

    public override Godot.StringName GetDecisionState(Node2D target)
    {
        if (target != null)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.FleeStateName;
        }
        return IslandSurvivor.Logic.StateMachine.StateConstants.WanderStateName;
    }"""

content = re.sub(r'    public override string CurrentState => m_passiveController\?\.CurrentState \?\? NpcStates\.IDLE;', replacement, content)

with open('./Src/IslandSurvivor/Scenes/NPC/Passive/PassiveNpcBase.cs', 'w') as f:
    f.write(content)
