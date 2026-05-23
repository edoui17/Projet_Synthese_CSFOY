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

    [ExportGroup("Audio Override")]
    [Export] public AudioStream? AttackSound { get; set; }
    [Export] public string AttackSoundKey { get; set; } = "Enemy_Swing_Default";
    [Export] public float AttackVolume { get; set; } = 1.0f;

    protected IAgressorController m_agressorController;
    protected AttackController? m_attackController;
    protected Area2D m_detectionArea;
    protected RayCast2D m_lineOfSightRay;
    protected Node2D m_targetPlayer;

    public Node2D GetTarget() => m_targetPlayer;
    public bool HasTargetAndLineOfSight() => m_targetPlayer != null && CheckLineOfSight();

    protected StringName m_animMoving = new StringName("Moving");
    protected StringName m_animIdle = new StringName("Idle");
    public string EnemyType => "GenericEnemy";

    public override void _Ready()
    {
        base._Ready();

        // Initialize default keys if not set
        if (string.IsNullOrEmpty(HurtSoundKey)) HurtSoundKey = "Enemy_Hurt_Default";
        if (string.IsNullOrEmpty(DeathSoundKey)) DeathSoundKey = "Enemy_Death_Default";

        InitializeController();

        m_attackController = GetNodeOrNull<AttackController>("AttackController");
        if (m_animatedSprite != null && m_attackController != null)
        {
            if (m_attackController.AttackSprite == null)
            {
                m_attackController.AttackSprite = m_animatedSprite;
            }
            m_attackController.AttackActionTriggered += PlayAttackSound;
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
        base._PhysicsProcess(p_delta);

        if (m_movementController == null && Velocity != Vector2.Zero)
        {
            MoveAndSlide();
        }
    }

    protected virtual void PlayAttackAnimation(string p_animName)
    {
        if (m_animatedSprite == null) return;

        if (m_targetPlayer != null)
        {
            m_animatedSprite.FlipH = m_targetPlayer.GlobalPosition.X < GlobalPosition.X;
        }

        if (m_attackController != null)
        {
            m_attackController.SetAttackAnimation(p_animName);
        }

        m_animatedSprite.Play(p_animName);
        m_animatedSprite.Frame = 0;
    }

    protected virtual void UpdateAnimation(Vector2 p_direction)
    {
        if (m_animatedSprite == null) return;

        bool isAttacking = m_attackController != null && m_attackController.IsAttacking;

        if (isAttacking)
        {
            // Attack animation playback is handled directly via PlayAttackAnimation in OnAttackStarted.
            // We just skip standard directional animation logic here.
            return;
        }

        if (Velocity.LengthSquared() > 0)
        {
            if (m_animatedSprite.Animation != m_animMoving)
            {
                m_animatedSprite.Play(m_animMoving);
            }
        }
        else
        {
            if (m_animatedSprite.Animation != m_animIdle)
            {
                m_animatedSprite.Play(m_animIdle);
            }
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

    protected void PlayAttackSound()
    {
        if (AttackSound != null)
            AudioManager.Instance?.PlaySound2D(AttackSound, GlobalPosition, p_volumeLinear: AttackVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
        else if (!string.IsNullOrEmpty(AttackSoundKey))
            AudioManager.Instance?.PlaySound2D(AttackSoundKey, GlobalPosition, p_volumeLinear: AttackVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
    }

    protected override void OnDamageTaken(Node2D p_attacker)
    {
        base.OnDamageTaken(p_attacker);
        m_targetPlayer = p_attacker;

        if (HurtSound != null)
            AudioManager.Instance?.PlaySound2D(HurtSound, GlobalPosition, p_volumeLinear: HurtVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
        else if (!string.IsNullOrEmpty(HurtSoundKey))
            AudioManager.Instance?.PlaySound2D(HurtSoundKey, GlobalPosition, p_volumeLinear: HurtVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
    }

    protected override void HandleDeath(object? p_attacker = null)
    {
        m_stateMachine?.ForceTransition("DeathState");

        if (DeathSound != null)
            AudioManager.Instance?.PlaySound2D(DeathSound, GlobalPosition, p_volumeLinear: DeathVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
        else if (!string.IsNullOrEmpty(DeathSoundKey))
            AudioManager.Instance?.PlaySound2D(DeathSoundKey, GlobalPosition, p_volumeLinear: DeathVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);

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
