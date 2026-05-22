namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;
using IslandSurvivor.Logic.Entities;

public partial class BossBase : AggressiveNpcBase
{
    [Export] public PackedScene ProjectileScene { get; set; }

    [ExportGroup("Animations")]
    [Export] public string MeleeAttackAnimationName { get; set; } = "Attack_Melee";
    [Export] public string RangedAttackAnimationName { get; set; } = "Attack_Ranged";

    protected Area2D m_hitboxAreaRight;
    protected Area2D m_hitboxAreaLeft;
    protected IBossController m_bossController;

    public override void _Ready()
    {
        base._Ready();

        m_hitboxAreaRight = GetNodeOrNull<Area2D>("HitboxAreaRight");
        m_hitboxAreaLeft = GetNodeOrNull<Area2D>("HitboxAreaLeft");

        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;
            m_attackController.AttackSprite = m_animatedSprite;
            m_attackController.ActionFrame = 3; // Generic boss hit frame

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
        // Use the new BossController for phase management
        m_bossController = new BossController(StoppingDistance);
        m_agressorController = m_bossController;
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

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_agressorController.CurrentState == NpcStates.DEAD) return;

        bool hasLineOfSight = CheckLineOfSight();

        // Pass the health ratio to update boss phases
        float healthRatio = 1.0f;
        if (Stats != null && Stats.MaxHealth > 0)
        {
            healthRatio = Stats.GetCurrentValue(Core.Managers.Stats.StatType.Health) / Stats.MaxHealth;
        }

        m_bossController.UpdateBoss((float)p_delta, m_targetPlayer != null, hasLineOfSight, healthRatio);

        // Always check if we can attack
        HandleAttackState();

        Vector2 direction = new Vector2(m_agressorController.CurrentDirection.X, m_agressorController.CurrentDirection.Y);
        float targetSpeed = IdleSpeed;

        if (m_attackController != null && m_attackController.IsAttacking)
        {
            targetSpeed = 0f;
            direction = Vector2.Zero;
        }
        else if (m_agressorController.CurrentState == NpcStates.CHASE && m_targetPlayer != null)
        {
            float distanceSquaredToPlayer = GlobalPosition.DistanceSquaredTo(m_targetPlayer.GlobalPosition);
            // Replaced DistanceTo with DistanceSquaredTo to eliminate square root calculation in hot path (_PhysicsProcess)
            if (distanceSquaredToPlayer <= StoppingDistance * StoppingDistance)
            {
                targetSpeed = 0f;
                direction = Vector2.Zero;
            }
            else
            {
                targetSpeed = ChaseSpeed;
                Vector2 globalPos = GlobalPosition;
                Vector2 targetPos = m_targetPlayer.GlobalPosition;
                m_agressorController.UpdateChaseDirection(globalPos, targetPos);
                direction = new Vector2(m_agressorController.CurrentDirection.X, m_agressorController.CurrentDirection.Y);
            }
        }

        if (m_movementController != null)
        {
            m_movementController.Move(direction, targetSpeed);
        }
        else
        {
            Velocity = direction * targetSpeed;
            MoveAndSlide();
        }

        if (m_agressorController.CurrentState == NpcStates.IDLE && GetSlideCollisionCount() > 0)
        {
            m_agressorController.ForceNewDirection();
        }

        UpdateAnimation(new Vector2(m_agressorController.CurrentDirection.X, m_agressorController.CurrentDirection.Y));
    }

    protected override void HandleAttackState()
    {
        if (m_attackController != null && m_attackController.CanAttack && m_targetPlayer != null)
        {
            float distanceSquaredToPlayer = GlobalPosition.DistanceSquaredTo(m_targetPlayer.GlobalPosition);

            // Phase logic to decide between Melee and Ranged
            if (m_bossController.CurrentPhase == BossPhase.Ranged)
            {
                // Ranged attack if in sight and within a reasonable distance
                if (distanceSquaredToPlayer <= 400f * 400f && CheckLineOfSight())
                {
                    TryTriggerAttack("Ranged");
                }
            }
            else
            {
                // Melee attack if close enough
                if (distanceSquaredToPlayer <= 80f * 80f)
                {
                    TryTriggerAttack("Melee");
                }
            }
        }
    }

    private void TryTriggerAttack(string p_attackType)
    {
        if (m_animatedSprite != null && m_targetPlayer != null)
        {
            m_animatedSprite.FlipH = m_targetPlayer.GlobalPosition.X < GlobalPosition.X;
        }
        string direction = (m_animatedSprite != null && m_animatedSprite.FlipH) ? "Left" : "Right";
        m_attackController.TryAttack(direction);
    }

    protected virtual void OnAttackStarted()
    {
        string animName = m_bossController.CurrentPhase == BossPhase.Ranged ? RangedAttackAnimationName : MeleeAttackAnimationName;
        PlayAttackAnimation(animName);
    }

    protected virtual void OnAttackActionTriggered()
    {
        // Ranged attack action
        if (m_bossController.CurrentPhase == BossPhase.Ranged)
        {
            ShootProjectile();
        }
        // Melee attack action is handled automatically by the hitboxes in AttackController
    }

    protected virtual void ShootProjectile()
    {
        if (ProjectileScene == null || m_targetPlayer == null) return;

        Node projectileNode = ProjectileScene.Instantiate();
        if (projectileNode is IProjectile projectile)
        {
            Godot.Vector2 directionGodot = (m_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
            Vector2 directionNumerics = new Vector2(directionGodot.X, directionGodot.Y);

            Marker2D spawnPosNode = GetNodeOrNull<Marker2D>("ProjectileSpawnPosition");
            Godot.Vector2 spawnGodot = spawnPosNode != null ? spawnPosNode.GlobalPosition : GlobalPosition;
            Vector2 startPositionNumerics = new Vector2(spawnGodot.X, spawnGodot.Y);

            float damageAmount = Stats?.BaseAttackValue ?? 20.0f;

            projectile.Initialize(startPositionNumerics, directionNumerics, damageAmount, this);
            GetTree().CurrentScene.AddChild(projectileNode);
            projectile.Fire();
        }
    }
}
