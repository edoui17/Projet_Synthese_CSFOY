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
                    windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.RangedAttackStateName;
                }
                return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
            }

            // If shooter is on cooldown but we are outside melee range, we should chase
            // to get into melee range while ranged is cooling down.
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }
    }
}
