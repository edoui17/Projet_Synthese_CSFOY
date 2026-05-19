namespace IslandSurvivor.Scenes.NPC.Agressive;

using System;
using System.Collections.Generic;
using Godot;
using IslandSurvivor.Extensions;
using Core.Interfaces.Entities;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Nodes;
using IslandSurvivor.Nodes.Movement;
using IslandSurvivor.Globals; // For SignalManager, ServiceRegistry
using IslandSurvivor.Interfaces; // INpc
using Core.Interfaces; // IEnemy

public abstract partial class EnemyBase : CharacterBody2D, INpc, IEnemy, IDamageable
{
    [Export] public float IdleSpeed { get; set; } = 50.0f;
    [Export] public float ChaseSpeed { get; set; } = 100.0f;
    [Export] public float StoppingDistance { get; set; } = 40.0f;
    [Export] public int LevelIndex { get; set; } = 1;

    [ExportGroup("XP Settings")]
    [Export] public float BaseXp { get; set; } = 30.0f;
    [Export] public float XpMultiplier { get; set; } = 0.2f;

    [Export] protected StatManager Stats;

    protected IAgressorController m_agressorController;
    protected MovementController m_movementController;

    protected AnimatedSprite2D m_animatedSprite;
    protected Area2D m_detectionArea;
    protected RayCast2D m_lineOfSightRay;

    protected Node2D m_targetPlayer;

    protected bool m_wasKilledByPlayer = false;
    protected object m_lastAttacker = null;

    public string CurrentState => m_agressorController?.CurrentState ?? NpcStates.IDLE;

    // Implement INpc and IEnemy properties
    public string NpcType => "NPC";
    public string EnemyType => "GenericEnemy";

    public override void _Ready()
    {
        base._Ready();

        m_movementController = GetNodeOrNull<MovementController>("MovementController");

        // Initialize appropriate controller in child classes
        InitializeController();

        m_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (m_animatedSprite == null)
        {
            GD.PrintErr($"{Name} node requires an AnimatedSprite2D child node.");
        }
        else
        {
            var attackController = GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
            if (attackController != null && attackController.AttackSprite == null)
            {
                attackController.AttackSprite = m_animatedSprite;
            }
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
            Stats.Connect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
    }

    protected virtual void InitializeController()
    {
        m_agressorController = new AgressorController();
    }

    protected virtual void ApplyLevelScaling()
    {
        // Calculate scaling factor: BaseStat * (1 + 0.2 * (LevelIndex - 1))
        float scalingFactor = 1.0f + 0.2f * (LevelIndex - 1);

        float maxHealth = Stats.MaxHealth * scalingFactor;
        float baseDamage = Stats.BaseAttackValue * scalingFactor;

        // Apply scaling factor to speeds
        IdleSpeed *= scalingFactor;
        ChaseSpeed *= scalingFactor;

        // Apply visual modulation based on level
        if (m_animatedSprite != null)
        {
            Color modulateColor = Colors.White; // Default/Blue (Level 1-2)
            if (LevelIndex >= 3 && LevelIndex <= 5)
            {
                modulateColor = Colors.Yellow; // Normal
            }
            else if (LevelIndex >= 6 && LevelIndex <= 8)
            {
                modulateColor = Colors.Red; // Hard
            }
            else if (LevelIndex > 8 && LevelIndex <= 10)
            {
                modulateColor = Colors.Purple; // Very Difficult
            }
            else if (LevelIndex > 10)
            {
                modulateColor = Colors.DarkGray; // Big Threat
            }

            m_animatedSprite.SelfModulate = modulateColor;
        }

        // Set the new max health, current health, and damage
        Stats.MaxHealth = maxHealth;
        Stats.SetCurrentValue(StatType.Health, maxHealth);
        Stats.BaseAttackValue = baseDamage;
    }

    protected virtual void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        if ((StatType)p_statType == StatType.Health && p_currentValue <= 0)
        {
            if (Stats != null)
            {
                Stats.Disconnect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
            }
            HandleDeath(m_lastAttacker);
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_agressorController.CurrentState == NpcStates.DEAD) return;

        bool hasLineOfSight = CheckLineOfSight();

        // Let the controller update its state
        m_agressorController.Update((float)p_delta, m_targetPlayer != null, hasLineOfSight);

        // Always check if we can attack (this allows entering the ATTACK state)
        HandleAttackState();

        UpdateAnimation(new Vector2(m_agressorController.CurrentDirection.X, m_agressorController.CurrentDirection.Y));

        Vector2 direction = new Vector2(m_agressorController.CurrentDirection.X, m_agressorController.CurrentDirection.Y);
        float targetSpeed = IdleSpeed;

        var attackController = GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");

        if (attackController != null && attackController.IsAttacking)
        {
            // Do not move while attacking
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
                System.Numerics.Vector2 globalPositionNumerics = new System.Numerics.Vector2(GlobalPosition.X, GlobalPosition.Y);
                System.Numerics.Vector2 targetPositionNumerics = new System.Numerics.Vector2(m_targetPlayer.GlobalPosition.X, m_targetPlayer.GlobalPosition.Y);
                m_agressorController.UpdateChaseDirection(globalPositionNumerics, targetPositionNumerics);
                direction = new Vector2(m_agressorController.CurrentDirection.X, m_agressorController.CurrentDirection.Y);
            }
        }

        // Apply movement
        if (m_movementController != null)
        {
            m_movementController.Move(direction, targetSpeed);
        }
        else
        {
            Velocity = direction * targetSpeed;
            MoveAndSlide();
        }

        // Obstacle avoidance in IDLE state
        if (m_agressorController.CurrentState == NpcStates.IDLE && GetSlideCollisionCount() > 0)
        {
            m_agressorController.ForceNewDirection();
        }
    }

    protected virtual void HandleAttackState()
    {
        // Override in child classes for specific attack behavior
    }

    protected virtual void UpdateAnimation(Vector2 p_direction)
    {
        if (m_animatedSprite == null) return;

        var attackController = GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
        bool isAttacking = attackController != null && attackController.IsAttacking;

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
        if (m_targetPlayer == null || m_lineOfSightRay == null)
            return false;

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



    public virtual void TakeDamage(int p_amount, object p_attacker)
    {
        if (CurrentState == NpcStates.DEAD) return;

        m_lastAttacker = p_attacker;

        if (Stats != null)
        {
            float currentHp = Stats.GetCurrentValue(StatType.Health);
            Stats.SetCurrentValue(StatType.Health, currentHp - p_amount);
        }

        bool isDead = Stats == null || Stats.GetCurrentValue(StatType.Health) <= 0;

        if (p_attacker is Node2D attackerNode)
        {
            if (isDead && attackerNode.IsInGroup("Player"))
            {
                m_wasKilledByPlayer = true;
            }

            if (!isDead)
            {
                m_targetPlayer = attackerNode;
                this.PlayHitFlash();
                this.PlayShake();

                // Try to play enemy hurt sound
                AudioStream hurtStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/enemy_hurt.wav");
                if (hurtStream != null)
                {
                    AudioManager.Instance?.PlaySound2D(hurtStream, GlobalPosition);
                }
            }
        }
    }

    protected virtual void HandleDeath(object p_attacker = null)
    {
        m_agressorController.SetDead();

        // Try to play enemy death sound
        AudioStream deathStream = GD.Load<AudioStream>("res://Src/IslandSurvivor/Assets/Sounds/Combat/enemy_death.wav");
        if (deathStream != null)
        {
            AudioManager.Instance?.PlaySound2D(deathStream, GlobalPosition);
        }

        // Ennemies (Soldiers, Archers) only give score, no gold.

        if (ServiceRegistry.Instance != null)
        {
            float xpEarned = BaseXp + (BaseXp * (LevelIndex - 1) * XpMultiplier);
            ServiceRegistry.Instance.EventBus.Publish(new Core.Events.EnemyKilledEvent(Name, EnemyType, xpEarned));
            // Base score is 10. For every 3 levels, it multiplies.
            // Level 1-2 = 10, Level 3-5 = 20, Level 6-8 = 40, Level 9-10 = 80, Level 11+ = 160.
            int multiplier = (LevelIndex - 1) / 3;
            int baseScore = 10;
            int scoreToAward = baseScore * (int)Math.Pow(2, multiplier);

            ServiceRegistry.Instance.ScoreTracker.AddScore(scoreToAward);
            GD.Print($"[EnemyBase] Enemy died. Sent {scoreToAward} points to ScoreManager and published {xpEarned} XP.");
        }

        QueueFree();
    }
}
