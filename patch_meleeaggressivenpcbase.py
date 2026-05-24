import re

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/MeleeAggressiveNpcBase.cs', 'r') as f:
    content = f.read()

replacement = """    public override bool CanGuard => !m_isGuarding && m_guardCooldownTimer <= 0.0f;

    protected override Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        if (distanceSquared > attackRangeSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }

        if (m_attackController != null && m_attackController.CanAttack)
        {
            if (CanGuard && GD.Randf() <= GuardChance)
            {
                return IslandSurvivor.Logic.StateMachine.StateConstants.GuardStateName;
            }

            return IslandSurvivor.Logic.StateMachine.StateConstants.AttackStateName;
        }
        else
        {
            if (CanGuard)
            {
                return IslandSurvivor.Logic.StateMachine.StateConstants.GuardStateName;
            }
            else
            {
                return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
            }
        }
    }"""

content = re.sub(r'    public override bool CanGuard => !m_isGuarding && m_guardCooldownTimer <= 0\.0f;', replacement, content)

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/MeleeAggressiveNpcBase.cs', 'w') as f:
    f.write(content)
