import re

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.cs', 'r') as f:
    content = f.read()

replacement = """    public virtual bool CanGuard => false;

    public override Godot.StringName GetDecisionState(Node2D target)
    {
        if (target == null)
        {
            if (m_stateMachine != null)
            {
                if (m_stateMachine.HasState(IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName))
                {
                    var idleStateNode = m_stateMachine.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName);
                    if (idleStateNode is IslandSurvivor.Logic.StateMachine.States.IdleState idleState)
                    {
                        if (idleState.IsWanderCooldownElapsed && m_stateMachine.HasState(IslandSurvivor.Logic.StateMachine.StateConstants.WanderStateName))
                        {
                            return IslandSurvivor.Logic.StateMachine.StateConstants.WanderStateName;
                        }
                    }
                }
            }
            return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
        }

        float distanceSquared = GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
        float attackRangeSquared = AttackRange * AttackRange;

        return GetCombatDecisionState(distanceSquared, attackRangeSquared);
    }

    protected virtual Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        if (distanceSquared > attackRangeSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }

        if (m_attackController != null && m_attackController.CanAttack)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.AttackStateName;
        }

        return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
    }"""

content = re.sub(r'    public virtual bool CanGuard => false;', replacement, content)

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.cs', 'w') as f:
    f.write(content)
