using Core.Managers.Stats;


using Core.Domain;
namespace IslandSurvivor.Scenes.NPC.Aggressive;

using System;
using Godot;
using IslandSurvivor.Extensions;
using IslandSurvivor.Logic.Entities;
using Core.Interfaces.Stats;
using IslandSurvivor.Nodes.Combat;
using IslandSurvivor.Globals;
using Core.Interfaces;

public partial class AggressiveNpcBase : NpcBase, IEnemy
{
    [Export] public float ChaseSpeed { get; set; } = 100.0f;
    [Export] public float StoppingDistance { get; set; } = 40.0f;
    [Export] public int LevelIndex { get; set; } = 1;

    [ExportGroup("XP Settings")]
    [Export] public float BaseXp { get; set; } = 30.0f;
    [Export] public float XpMultiplier { get; set; } = 0.2f;

    protected IAgressorController m_agressorController;
    protected AttackController? m_attackController;
    protected Area2D m_detectionArea;
    protected RayCast2D m_lineOfSightRay;
    protected Node2D m_targetPlayer;

    public override string CurrentState => m_agressorController?.CurrentState ?? NpcStates.IDLE;
    public string EnemyType => "GenericEnemy";

    public override void _Ready()
    {
        base._Ready();

        InitializeController();

        m_attackController = GetNodeOrNull<AttackController>("AttackController");
        if (m_animatedSprite != null && m_attackController != null && m_attackController.AttackSprite == null)
        {
            m_attackController.AttackSprite = m_animatedSprite;
        }

        m_detectionArea = GetNodeOrNull<Area2D>("DetectionArea");
        if (m_detectionArea == null)
        {
            GD.PrintErr($"{Name} node requires an Area2D child node named 'DetectionArea'.");
        }
        else
        {
            m_detectionArea.BodyEntered += OnDetectionAreaBodyEntered;
            m_detectionArea.BodyExited += OnDetectionAreaBodyExited;
        }

        m_lineOfSightRay = GetNodeOrNull<RayCast2D>("LineOfSightRay");
        if (m_lineOfSightRay == null)
        {
            GD.PrintErr($"{Name} node requires a RayCast2D child node named 'LineOfSightRay'.");
        }

        if (Stats != null)
        {
            ApplyLevelScaling();
        }
    }

    protected virtual void InitializeController()
    {
        m_agressorController = new AgressorController();
    }

    protected virtual void ApplyLevelScaling()
    {
        float scalingFactor = 1.0f + 0.2f * (LevelIndex - 1);

        float maxHealth = Stats.MaxHealth * scalingFactor;
        float baseDamage = Stats.BaseAttackValue * scalingFactor;

        IdleSpeed *= scalingFactor;
        ChaseSpeed *= scalingFactor;

        if (m_animatedSprite != null)
        {
            Color modulateColor = Colors.White;
            if (LevelIndex >= 3 && LevelIndex <= 5) modulateColor = Colors.Yellow;
            else if (LevelIndex >= 6 && LevelIndex <= 8) modulateColor = Colors.Red;
            else if (LevelIndex > 8 && LevelIndex <= 10) modulateColor = Colors.Purple;
            else if (LevelIndex > 10) modulateColor = Colors.DarkGray;

            m_animatedSprite.SelfModulate = modulateColor;
        }

        Stats.MaxHealth = maxHealth;
        Stats.SetCurrentValue(StatType.Health, maxHealth);
        Stats.BaseAttackValue = baseDamage;
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_agressorController.CurrentState == NpcStates.DEAD) return;

        bool hasLineOfSight = CheckLineOfSight();

        m_agressorController.Update((float)p_delta, m_targetPlayer != null, hasLineOfSight);

        HandleAttackState();

        UpdateAnimation(new Vector2(m_agressorController.CurrentDirection.X, m_agressorController.CurrentDirection.Y));

        Vector2 direction = new Vector2(m_agressorController.CurrentDirection.X, m_agressorController.CurrentDirection.Y);
        float targetSpeed = IdleSpeed;

        if (m_attackController != null && m_attackController.IsAttacking)
        {
            targetSpeed = 0f;
            direction = Vector2.Zero;
        }
        else if (m_agressorController.CurrentState == NpcStates.CHASE && m_targetPlayer != null)
        {
            float distanceToPlayer = GlobalPosition.DistanceTo(m_targetPlayer.GlobalPosition);
            if (distanceToPlayer <= StoppingDistance)
            {
                targetSpeed = 0f;
                direction = Vector2.Zero;
            }
            else
            {
                targetSpeed = ChaseSpeed;
                Vector2 globalPositionNumerics = GlobalPosition;
                Vector2 targetPositionNumerics = m_targetPlayer.GlobalPosition;
                m_agressorController.UpdateChaseDirection(globalPositionNumerics, targetPositionNumerics);
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
    }

    protected virtual void HandleAttackState()
    {
    }

    protected virtual void UpdateAnimation(Vector2 p_direction)
    {
        if (m_animatedSprite == null) return;

        bool isAttacking = m_attackController != null && m_attackController.IsAttacking;

        if (isAttacking)
        {
            if (m_animatedSprite.Animation != "Attack")
            {
                m_animatedSprite.Play("Attack");
                m_animatedSprite.Frame = 0;
            }
            return;
        }

        if (Velocity.LengthSquared() > 0)
        {
            m_animatedSprite.Play("Moving");
        }
        else
        {
            m_animatedSprite.Play("Idle");
        }

        if (p_direction.X != 0 && !isAttacking)
        {
            m_animatedSprite.FlipH = p_direction.X < 0;
        }
    }

    protected virtual bool CheckLineOfSight()
    {
        if (m_targetPlayer == null || m_lineOfSightRay == null) return false;

        Vector2 targetLocalPosition = ToLocal(m_targetPlayer.GlobalPosition);
        m_lineOfSightRay.TargetPosition = targetLocalPosition;
        m_lineOfSightRay.ForceRaycastUpdate();

        if (m_lineOfSightRay.IsColliding())
        {
            GodotObject collider = m_lineOfSightRay.GetCollider();
            if (collider is Node2D node && (node.IsInGroup("Player") || node.Name == "Player"))
            {
                return true;
            }
            return false;
        }
        return true;
    }

    protected virtual void OnDetectionAreaBodyEntered(Node2D p_body)
    {
        if (p_body.IsInGroup("Player") || p_body.Name == "Player")
        {
            m_targetPlayer = p_body;
        }
    }

    protected virtual void OnDetectionAreaBodyExited(Node2D p_body)
    {
        if (p_body == m_targetPlayer)
        {
            m_targetPlayer = null;
        }
    }

    protected override void OnDamageTaken(Node2D p_attacker)
    {
        base.OnDamageTaken(p_attacker);
        m_targetPlayer = p_attacker;

        AudioStream hurtStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/enemy_hurt.wav");
        if (hurtStream != null)
        {
            AudioManager.Instance?.PlaySound2D(hurtStream, GlobalPosition);
        }
    }

    protected override void HandleDeath(object? p_attacker = null)
    {
        m_agressorController.SetDead();

        AudioStream deathStream = GD.Load<AudioStream>("res://Src/IslandSurvivor/Assets/Sounds/Combat/enemy_death.wav");
        if (deathStream != null)
        {
            AudioManager.Instance?.PlaySound2D(deathStream, GlobalPosition);
        }

        if (ServiceRegistry.Instance != null)
        {
            float xpEarned = BaseXp + (BaseXp * (LevelIndex - 1) * XpMultiplier);
            ServiceRegistry.Instance.EventBus.Publish(new Core.Events.EnemyKilledEvent(Name, EnemyType, xpEarned));

            int multiplier = (LevelIndex - 1) / 3;
            int baseScore = 10;
            int scoreToAward = baseScore * (int)Math.Pow(2, multiplier);

            ServiceRegistry.Instance.ScoreTracker.AddScore(scoreToAward);
            GD.Print($"[AggressiveNpcBase] Enemy died. Sent {scoreToAward} points to ScoreManager and published {xpEarned} XP.");
        }

        QueueFree();
    }
}
