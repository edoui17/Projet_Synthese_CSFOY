file_path = 'Src/IslandSurvivor/Scenes/NPC/Aggressive/Boss/Reaper/Reaper.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

# Insert GetCombatDecisionState to alternate based on range
get_decision_code = """
    protected override Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        float maxAttackRangeSquared = MaxAttackRange * MaxAttackRange;
        float minAttackRangeSquared = MinAttackRange * MinAttackRange;

        if (distanceSquared > maxAttackRangeSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }

        // Reaper specific logic: Melee when close, Ranged when far.
        // Assuming MinAttackRange acts as the Melee range.
        if (distanceSquared <= minAttackRangeSquared)
        {
            if (m_attackController != null && m_attackController.CanAttack)
            {
                var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
                if (windUp != null)
                {
                    windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.MeleeAttackStateName;
                }
                return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
            }
        }
        else
        {
            var shooter = GetNodeOrNull<IslandSurvivor.Nodes.Combat.Shooter>("Shooter");
            if (shooter != null && shooter.CanShoot)
            {
                var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
                if (windUp != null)
                {
                    windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.RangedAttackStateName;
                }
                return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
            }

            // If shooter is on cooldown, chase or idle?
            if (distanceSquared > minAttackRangeSquared)
            {
                return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
            }
        }

        return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
    }
"""

content = re.sub(r'public override void _Ready\(\)', get_decision_code + '\n    public override void _Ready()', content)

with open(file_path, 'w') as f:
    f.write(content)
