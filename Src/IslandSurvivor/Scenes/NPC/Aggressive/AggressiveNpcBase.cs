namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Globals;

public partial class AggressiveNpcBase : NpcBase
{
    [Export] public float ChaseSpeed { get; set; } = 120.0f;
    [Export] public float StoppingDistance { get; set; } = 50.0f;
    [Export] public int LevelIndex { get; set; } = 1;
    [Export] public float BaseXp { get; set; } = 10.0f;
    [Export] public float XpMultiplier { get; set; } = 0.5f;

    [ExportGroup("Combat Audio")]
    [Export] public AudioStream? AttackSound { get; set; }
    [Export] public string AttackSoundKey { get; set; } = string.Empty;
    [Export] public float AttackVolume { get; set; } = 1.0f;

    [ExportGroup("Melee Configuration")]
    [Export] public float AttackRange { get; set; } = 60.0f;
    [Export] public float GuardChance { get; set; } = 0.3f;

    [ExportGroup("Ranged Configuration")]
    [Export] public float MinAttackRange { get; set; } = 80.0f;
    [Export] public float MaxAttackRange { get; set; } = 350.0f;

    protected Node2D? m_targetPlayer;
    // protected IAgressorController m_agressorController = null!;
    protected IslandSurvivor.Nodes.Combat.AttackController? m_attackController;
    protected Area2D? m_detectionArea;
    protected RayCast2D? m_lineOfSightRay;
    protected Sprite2D? m_sprite;
    protected AnimationPlayer? m_animationPlayer;

    public Vector2 LockedDirection { get; set; } = Vector2.Zero;

    protected bool m_isGuarding = false;
    protected string m_guardDirection = "";
    protected float m_guardDamageMultiplier = 1.0f;
    protected float m_guardCooldownTimer = 0.0f;

    public virtual bool CanGuard => false;

    public override Godot.StringName GetDecisionState(Node2D target)
    {
        if (target == null)
        {
            if (m_stateMachine != null)
            {
                if (m_stateMachine.HasState(IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName))
                {
                    var idleStateNode = m_stateMachine.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName);
                    if (idleStateNode is IslandSurvivor.Logic.StateMachine.States.IdleState idleState)
                    {
                        if (idleState.IsWanderCooldownElapsed && m_stateMachine.HasState(IslandSurvivor.Logic.StateMachine.StateConstants.WanderStateName))
                        {
                            return IslandSurvivor.Logic.StateMachine.StateConstants.WanderStateName;
                        }
                    }
                }
            }
            return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
        }

        float distanceSquared = GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
        float attackRangeSquared = AttackRange * AttackRange;

        return GetCombatDecisionState(distanceSquared, attackRangeSquared);
    }

    protected virtual Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        if (distanceSquared > attackRangeSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }

        if (m_attackController != null && m_attackController.CanAttack)
        {
            var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
            if (windUp != null)
            {
                // State routing is handled by specific NpcBase
            }
            return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
        }

        return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
    }

    public void SetGuardState(bool isGuarding, string direction, float multiplier)
    {
        m_isGuarding = isGuarding;
        m_guardDirection = direction;
        m_guardDamageMultiplier = multiplier;
    }

    public void StartGuardCooldown(float cooldown)
    {
        m_guardCooldownTimer = cooldown;
    }


    public Node2D? GetTarget() => m_targetPlayer;
    public bool HasTargetAndLineOfSight() => m_targetPlayer != null && CheckLineOfSight();

    public string EnemyType => "GenericEnemy";

    public override void _Ready()
    {
        base._Ready();
        m_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        m_animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

        // Initialize default keys if not set
        if (string.IsNullOrEmpty(HurtSoundKey)) HurtSoundKey = "Enemy_Hurt_Default";
        if (string.IsNullOrEmpty(DeathSoundKey)) DeathSoundKey = "Enemy_Death_Default";

        InitializeController();

        m_attackController = GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
        if (m_sprite != null && m_attackController != null)
        {
            if (m_attackController.AttackSprite == null)
            {
                m_attackController.AttackSprite = m_sprite;
                m_attackController.AttackAnimationPlayer = m_animationPlayer;
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

        var levelLabel = GetNodeOrNull<Label>("LevelLabel");
        if (levelLabel != null)
        {
            levelLabel.Text = $"Lvl {LevelIndex}";
        }
    }

    protected virtual void InitializeController()
    {
        // m_agressorController = new AgressorController();
    }

    protected virtual void ApplyLevelScaling()
    {
        // Stats scaling is now purely handled by the EnemyStatsHandler Decorator
        // using the Global Threat Score system. We only apply the color modulation here
        // as a visual indicator of the enemy's raw level before map multipliers.

        if (m_sprite != null)
        {
            Color modulateColor = Colors.White;
            if (LevelIndex >= 3 && LevelIndex <= 5) modulateColor = Colors.Yellow;
            else if (LevelIndex >= 6 && LevelIndex <= 8) modulateColor = Colors.Red;
            else if (LevelIndex > 8 && LevelIndex <= 10) modulateColor = Colors.Purple;
            else if (LevelIndex > 10) modulateColor = Colors.DarkGray;

            m_sprite.SelfModulate = modulateColor;
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        base._PhysicsProcess(p_delta);

        if (m_guardCooldownTimer > 0)
        {
            m_guardCooldownTimer -= (float)p_delta;
        }
    }

    protected virtual void PlayAttackAnimation(string p_animName)
    {
        if (m_sprite == null) return;

        if (m_targetPlayer != null)
        {
            m_sprite.FlipH = m_targetPlayer.GlobalPosition.X < GlobalPosition.X;
        }

        if (m_attackController != null)
        {
            m_attackController.SetAttackAnimation(p_animName);
        }

        m_animationPlayer?.Play(p_animName);
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

    public override void TakeDamage(int p_amount, object p_attacker)
    {
        m_lastAttacker = p_attacker;

        int actualDamage = p_amount;
        bool wasBlocked = false;

        if (m_isGuarding && p_attacker is Node2D attackNode)
        {
            // Determine attack direction relative to the NPC
            bool attackCameFromLeft = attackNode.GlobalPosition.X < GlobalPosition.X;
            string attackDirection = attackCameFromLeft ? "Left" : "Right";

            if (attackDirection == m_guardDirection)
            {
                wasBlocked = true;
                actualDamage = Mathf.RoundToInt(p_amount * m_guardDamageMultiplier);
            }
        }

        if (Stats != null)
        {
            float currentHp = Stats.GetCurrentValue(Core.Managers.Stats.StatType.Health);
            Stats.SetCurrentValue(Core.Managers.Stats.StatType.Health, currentHp - actualDamage);
        }

        bool isDead = Stats == null || Stats.GetCurrentValue(Core.Managers.Stats.StatType.Health) <= 0;

        if (p_attacker is Node2D attackerNode)
        {
            if (isDead && attackerNode.IsInGroup("Player"))
            {
                m_wasKilledByPlayer = true;
            }

            if (!isDead)
            {
                OnDamageTaken(attackerNode);

                if (wasBlocked)
                {
                    IslandSurvivor.Extensions.NodeExtensions.PlayShake(this);
                }
                else
                {
                    IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);
                    IslandSurvivor.Extensions.NodeExtensions.PlayShake(this);
                }
            }
        }
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
            int scoreToAward = baseScore * (int)System.Math.Pow(2, multiplier);

            ServiceRegistry.Instance.ScoreTracker.AddScore(scoreToAward);
            GD.Print($"[AggressiveNpcBase] Enemy died. Sent {scoreToAward} points to ScoreManager and published {xpEarned} XP.");
        }


    }
}
