namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;
using IslandSurvivor.Logic.Entities;

public partial class BossBase : AggressiveNpcBase
{
    [Export] public PackedScene ProjectileScene { get; set; } = null!;

    [ExportGroup("Animations")]
    [Export] public string MeleeAttackAnimationName { get; set; } = "Attack_Melee";
    [Export] public string RangedAttackAnimationName { get; set; } = "Attack_Ranged";

    protected Area2D? m_hitboxAreaRight;
    protected Area2D? m_hitboxAreaLeft;

    protected override Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        // Future boss phases and AoE cooldown logic will be injected here.
        return base.GetCombatDecisionState(distanceSquared, attackRangeSquared);
    }

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
        ChaseSpeed *= scalingFactor;

        Stats.MaxHealth = maxHealth;
        Stats.SetCurrentValue(Core.Managers.Stats.StatType.Health, maxHealth);
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
        ShootProjectile();
    }

    protected virtual void ShootProjectile()
    {
        if (ProjectileScene == null || m_targetPlayer == null) return;

        Node projectileNode = ProjectileScene.Instantiate();
        if (projectileNode is IProjectile projectile)
        {
            Godot.Vector2 directionGodot = (m_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
            Vector2 directionNumerics = new Vector2(directionGodot.X, directionGodot.Y);

            Marker2D? spawnPosNode = GetNodeOrNull<Marker2D>("ProjectileSpawnPosition");
            Godot.Vector2 spawnGodot = spawnPosNode != null ? spawnPosNode.GlobalPosition : GlobalPosition;
            Vector2 startPositionNumerics = new Vector2(spawnGodot.X, spawnGodot.Y);

            float damageAmount = Stats?.BaseAttackValue ?? 20.0f;

            projectile.Initialize(startPositionNumerics, directionNumerics, damageAmount, this);
            GetTree().CurrentScene.AddChild(projectileNode);
            projectile.Fire();
        }
    }

    protected override void HandleDeath(object? p_attacker = null)
    {
        base.HandleDeath(p_attacker);

        if (IslandSurvivor.Globals.ServiceRegistry.Instance != null && IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus != null)
        {
            IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Publish(new Core.Events.BossDiedEvent(Name, EnemyType));
        }
    }
}
