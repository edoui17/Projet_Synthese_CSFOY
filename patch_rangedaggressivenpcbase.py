import re

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/RangedAggressiveNpcBase.cs', 'r') as f:
    content = f.read()

replacement = """    [ExportGroup("Animations")]
    [Export] public string AttackAnimationName { get; set; } = "Attack";

    protected override Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        float maxAttackRangeSquared = MaxAttackRange * MaxAttackRange;
        float minAttackRangeSquared = MinAttackRange * MinAttackRange;

        if (distanceSquared > maxAttackRangeSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }

        if (distanceSquared < minAttackRangeSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.RepositionStateName;
        }

        if (m_attackController != null && m_attackController.CanAttack)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.AttackStateName;
        }

        return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
    }"""

content = re.sub(r'    \[ExportGroup\("Animations"\)\]\n    \[Export\] public string AttackAnimationName \{ get; set; \} = "Attack";', replacement, content)

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/RangedAggressiveNpcBase.cs', 'w') as f:
    f.write(content)
