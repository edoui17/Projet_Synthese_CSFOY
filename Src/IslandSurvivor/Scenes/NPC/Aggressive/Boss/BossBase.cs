namespace IslandSurvivor.Scenes.NPC;

using Godot;
using IslandSurvivor.Logic;

public partial class BossBase : AggressiveNpcBase
{
    [ExportGroup("Animations")]
    [Export] public string MeleeAttackAnimationName { get; set; } = "Attack_Melee";
    [Export] public string RangedAttackAnimationName { get; set; } = "Attack_Ranged";

    protected Area2D? m_hitboxAreaRight;
    protected Area2D? m_hitboxAreaLeft;


    public override void _Ready()
    {
        base._Ready();

        m_hitboxAreaRight = GetNodeOrNull<Area2D>("HitboxAreaRight");
        m_hitboxAreaLeft = GetNodeOrNull<Area2D>("HitboxAreaLeft");

        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;

            if (m_hitboxAreaRight != null) m_attackController.RegisterArea("Right", m_hitboxAreaRight);
            if (m_hitboxAreaLeft != null) m_attackController.RegisterArea("Left", m_hitboxAreaLeft);

            m_attackController.AttackStarted += OnAttackStarted;
            m_attackController.AttackActionTriggered += OnAttackActionTriggered;
        }
        else
        {
            GD.PushWarning($"{Name}: AttackController not found.");
        }
    }

    protected override void InitializeController()
    {
        // Controllers removed in favor of state machines.
    }

    protected override void ApplyLevelScaling()
    {
        // Bosses scale stats but do NOT tint color based on level.
        float scalingFactor = 1.0f + 0.3f * (LevelIndex - 1); // Maybe slightly higher scaling for boss
        float maxHealth = Stats.MaxHealth * scalingFactor;
        float baseDamage = Stats.BaseAttackValue * scalingFactor;

        IdleSpeed *= scalingFactor;
        if (m_stateMachine != null && m_stateMachine.TryGetState<IslandSurvivor.Logic.StateMachine.ChaseState>(out var chaseState)) chaseState.ChaseSpeed *= scalingFactor;

        Stats.MaxHealth = maxHealth;
        Stats.SetCurrentValue(Core.Managers.StatType.Health, maxHealth);
        Stats.BaseAttackValue = baseDamage;
    }

    protected virtual void OnAttackStarted()
    {
        // Handled by state machine
    }

    protected virtual void OnAttackActionTriggered()
    {
        // Melee attack action is handled automatically by the hitboxes in AttackController
        // Ranged attacks can trigger ShootProjectile()

    }

    protected override void HandleDeath(object? p_attacker = null)
    {
        base.HandleDeath(p_attacker);
    }

    protected override void OnDeathStateFinished(IslandSurvivor.Logic.StateMachine.State p_sourceState, IslandSurvivor.Logic.StateMachine.StateExitReason p_reason)
    {
        if (p_reason == IslandSurvivor.Logic.StateMachine.StateExitReason.Finished)
        {
            if (IslandSurvivor.Globals.ServiceRegistry.Instance != null && IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus != null)
            {
                IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Publish(new Core.Events.BossDiedEvent(Name, EnemyType));
            }
        }

        base.OnDeathStateFinished(p_sourceState, p_reason);
    }
}
