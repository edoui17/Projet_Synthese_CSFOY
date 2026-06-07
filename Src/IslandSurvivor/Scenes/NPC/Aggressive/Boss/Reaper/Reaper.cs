using Godot;

namespace IslandSurvivor.Scenes.NPC;

public partial class Reaper : BossBase
{
    [ExportGroup("Reaper Animations")]
    [Export] public string NormalAttackAnimationName { get; set; } = "MeleeAttackNormal";
    [Export] public string EnragedAttackAnimationName { get; set; } = "MeleeAttackEnraged";

    protected override Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        float maxAttackRangeSquared = MaxAttackRange * MaxAttackRange;
        float minAttackRangeSquared = MinAttackRange * MinAttackRange;

        // If outside ranged distance, close in.
        if (distanceSquared > maxAttackRangeSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }

        // If in melee range, try to melee attack.
        if (distanceSquared <= minAttackRangeSquared)
        {
            if (m_attackController != null && m_attackController.CanAttack)
            {
                var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.WindUpState;
                if (windUp != null)
                {
                    windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.MeleeAttackStateName;
                }
                return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
            }

            // On melee cooldown, wait/idle.
            return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
        }
        else
        {
            // Inside max range but outside melee range: try to shoot.
            var shooter = GetNodeOrNull<IslandSurvivor.Nodes.Shooter>("Shooter");
            if (shooter != null && shooter.CanShoot)
            {
                var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.WindUpState;
                if (windUp != null)
                {
                    // 50% chance to use MagicAttackState instead of standard RangedAttackState
                    if (GD.Randf() > 0.5f && m_stateMachine != null && m_stateMachine.HasState(IslandSurvivor.Logic.StateMachine.StateConstants.MagicAttackStateName))
                    {
                        windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.MagicAttackStateName;
                    }
                    else
                    {
                        windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.RangedAttackStateName;
                    }
                }

                // Put shooter on cooldown artificially when we commit to windup to prevent rapid firing.
                // Normally Shooter node manages this but we share cooldown for MagicAttack
                // Note: Shooter's cooldown logic is internal, so we trigger a dummy shot or let Shooter update natively.

                return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
            }

            // If shooter is on cooldown, check if we can melee attack before deciding to chase into melee range
            if (m_attackController != null && m_attackController.CanAttack)
            {
                return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
            }

            // Both ranged and melee are on cooldown, wait/idle where we are instead of pointless chasing.
            return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
        }
    }
}
