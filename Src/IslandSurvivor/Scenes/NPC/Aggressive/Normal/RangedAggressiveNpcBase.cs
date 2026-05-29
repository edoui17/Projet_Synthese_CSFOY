namespace IslandSurvivor.Scenes.NPC;

using Godot;
using IslandSurvivor.Logic;
using IslandSurvivor.Globals;

public partial class RangedAggressiveNpcBase : AggressiveNpcBase
{
    [ExportGroup("Animations")]
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
        else if (m_attackController != null && m_attackController.CanAttack)
        {
            var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
            if (windUp != null)
            {
                windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.RangedAttackStateName;
            }
            return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
        }

        return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
    }

    public override void _Ready()
    {
        base._Ready();

        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;

            m_attackController.AttackStarted += OnAttackStarted;

        }
        else
        {
            GD.PushWarning($"{Name}: AttackController not found.");
        }
    }

    protected virtual void OnAttackStarted()
    {
    }


}
