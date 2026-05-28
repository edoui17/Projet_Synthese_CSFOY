namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;

public partial class MeleeAggressiveNpcBase : AggressiveNpcBase
{
    public override bool CanGuard => !m_isGuarding && m_guardCooldownTimer <= 0.0f;

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

            var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
            if (windUp != null)
            {
                windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.MeleeAttackStateName;
            }
            return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
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
    }

    protected Area2D? m_hitboxAreaRight;
    protected Area2D? m_hitboxAreaLeft;
    protected Area2D? m_hitboxAreaUp;
    protected Area2D? m_hitboxAreaDown;

    public override void _Ready()
    {
        base._Ready();

        m_hitboxAreaRight = GetNodeOrNull<Area2D>("HitboxAreaRight");
        m_hitboxAreaLeft = GetNodeOrNull<Area2D>("HitboxAreaLeft");
        m_hitboxAreaUp = GetNodeOrNull<Area2D>("HitboxAreaUp");
        m_hitboxAreaDown = GetNodeOrNull<Area2D>("HitboxAreaDown");

        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;

            if (m_hitboxAreaRight != null) m_attackController.RegisterArea("Right", m_hitboxAreaRight);
            if (m_hitboxAreaLeft != null) m_attackController.RegisterArea("Left", m_hitboxAreaLeft);
            if (m_hitboxAreaUp != null) m_attackController.RegisterArea("Up", m_hitboxAreaUp);
            if (m_hitboxAreaDown != null) m_attackController.RegisterArea("Down", m_hitboxAreaDown);

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
