using Core.Interfaces;
using Core.Managers;
using Godot;
using IslandSurvivor.Extensions;
using IslandSurvivor.Globals;
using IslandSurvivor.Enums;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Managers;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using IslandSurvivor.Nodes;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, IDamageable
{
    [Export] public StatManager? Stats { get; set; }
    public string NpcType { get; set; } = "Player";

    private PlayerState m_currentState = PlayerState.Idle;
    private PlayerState m_lastDebugState = (PlayerState)(-1);


    [Export] private Sprite2D? m_sprite;
    [Export] private AnimationPlayer? m_animationPlayer;
    [Export] private Label? m_interactionLabel;
    [Export] private Label? m_debugLabel;
    [Export] private Label? m_levelLabel;
    [Export] private Label? m_xpGainLabel;
    [Export] private Label? m_dashLabel;
    private Timer? m_xpGainTimer;
    [Export] private Area2D? m_interactionArea;

    [ExportGroup("Audio")]
    [Export] public AudioStream? AttackSound { get; set; }
    [Export] public string AttackSoundKey { get; set; } = "Player_Swing";
    [Export] public float AttackVolume { get; set; } = 1.0f;

    [Export] public AudioStream? HurtSound { get; set; }
    [Export] public string HurtSoundKey { get; set; } = "Player_Hurt";
    [Export] public float HurtVolume { get; set; } = 0.3f; // Default was -10dB which is roughly 0.316 linear

    [Export] public AudioStream? DeathSound { get; set; }
    [Export] public string DeathSoundKey { get; set; } = "Player_Death";
    [Export] public float DeathVolume { get; set; } = 1.0f;

    [ExportGroup("Attack")]
    [Export] private Area2D? m_weaponAreaRight;
    [Export] private Area2D? m_weaponAreaLeft;

    private IslandSurvivor.Nodes.AttackController? m_attackController;

    private readonly List<IInteractable> m_nearbyInteractables = new();
    private IInteractable? m_bestTarget;
    private readonly IInteractionService m_interactionService = new InteractionService();
    private MovementController? m_movementController;

    // Cached StringNames to prevent implicit string allocation and GC spikes during engine interop calls
    private readonly StringName m_animIdle = new StringName("Idle");
    private readonly StringName m_animRun = new StringName("Moving");
    private readonly StringName m_animAttack = new StringName("Attack");
    private readonly StringName m_animInteract = new StringName("Interact");
    private readonly StringName m_animDeath = new StringName("Death");

    // Input actions
    private static readonly StringName ACTION_ATTACK = new StringName("attack");
    private static readonly StringName ACTION_INTERACT = new StringName("interact");
    private static readonly StringName ACTION_DASH = new StringName("dash");
    private static readonly StringName ACTION_MOVE_LEFT = new StringName("move_left");
    private static readonly StringName ACTION_MOVE_RIGHT = new StringName("move_right");
    private static readonly StringName ACTION_MOVE_UP = new StringName("move_up");
    private static readonly StringName ACTION_MOVE_DOWN = new StringName("move_down");

    private bool m_isAttackButtonDown = false;

    // State tracking to prevent per-frame string allocations during Dash UI updates
    private bool m_isDashReadyUI = false;
    private int m_lastDashDeciseconds = -1;

    public override void _ExitTree()
    {
        base._ExitTree();
        if (IslandSurvivor.Globals.ServiceRegistry.Instance != null && IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus != null)
        {
            IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Unsubscribe<Core.Events.LevelChangedEvent>(OnLevelChanged);
            IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Unsubscribe<Core.Events.ExperienceGainedEvent>(OnExperienceGained);
        }
    }

    public override void _Ready()
    {
        GD.Print("Attaque du joueur : " + Stats?.GetCurrentValue(StatType.Attack));
        if (m_interactionLabel != null) m_interactionLabel.Visible = false;

        if (m_xpGainLabel != null)
        {
            m_xpGainLabel.Visible = false;
        }

        m_xpGainTimer = new Timer();
        m_xpGainTimer.OneShot = true;
        m_xpGainTimer.WaitTime = 1.0f;
        m_xpGainTimer.Timeout += OnXpTimerTimeout;
        AddChild(m_xpGainTimer);

        IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Subscribe<Core.Events.LevelChangedEvent>(OnLevelChanged);
        IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Subscribe<Core.Events.ExperienceGainedEvent>(OnExperienceGained);

        UpdateLevelLabel();

        m_movementController = GetNodeOrNull<MovementController>("MovementController");

        if (Stats == null)
        {
            GD.PushWarning("Player: StatManager not assigned.");
        }

        if (m_interactionArea != null)
        {
            m_interactionArea.AreaEntered += OnInteractionAreaEntered;
            m_interactionArea.AreaExited += OnInteractionAreaExited;
        }

        m_attackController = GetNodeOrNull<IslandSurvivor.Nodes.AttackController>("AttackController");
        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = EntityFaction.Player;
            m_attackController.AttackSprite = m_sprite;
            m_attackController.AttackAnimationPlayer = m_animationPlayer;

            if (m_weaponAreaRight != null) m_attackController.RegisterArea("Right", m_weaponAreaRight);
            if (m_weaponAreaLeft != null) m_attackController.RegisterArea("Left", m_weaponAreaLeft);

            m_attackController.AttackStarted += OnAttackStarted;
            m_attackController.AttackFinished += OnAttackFinished;
        }
        else
        {
            GD.PushWarning("Player: AttackController not found.");
        }
    }

    private void OnAttackStarted()
    {
        // Play attack swing sound
        if (AttackSound != null)
            AudioManager.Instance?.PlaySound2D(AttackSound, GlobalPosition, p_volumeLinear: AttackVolume);
        else if (!string.IsNullOrEmpty(AttackSoundKey))
            AudioManager.Instance?.PlaySound2D(AttackSoundKey, GlobalPosition, p_volumeLinear: AttackVolume);

        if (m_animationPlayer != null && m_animationPlayer.HasAnimation(m_animAttack))
        {
            if (!m_animationPlayer.IsPlaying() || m_animationPlayer.CurrentAnimation != m_animAttack)
                m_animationPlayer.Play(m_animAttack);
        }
    }

    private void OnAttackFinished()
    {
        if (m_currentState == PlayerState.Attacking)
        {
            SetState(PlayerState.Idle);
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_currentState == PlayerState.Dead) return;

        if (m_debugLabel != null && m_currentState != m_lastDebugState)
        {
            m_debugLabel.Text = m_currentState.ToString();
            m_lastDebugState = m_currentState;
        }

        UpdateDashUI();

        if (m_movementController != null && m_movementController.IsDashing)
        {
            SetState(PlayerState.Dashing);
            return;
        }
        else if (m_currentState == PlayerState.Dashing)
        {
            SetState(PlayerState.Idle);
        }

        if (m_currentState == PlayerState.Interacting || m_currentState == PlayerState.Attacking)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            UpdateInteractionLabelPosition();
            return;
        }

        ApplyMovement();


        // Continue attack if button is held and we can attack
        // Prevent attack if clicking on UI by checking if any control has focus or mouse is captured by UI.
        // Actually, the simplest check in Godot 4 for this is `GetViewport().GuiGetFocusOwner() != null`
        // or just checking `Input.IsActionPressed` and skipping if UI is hovered.
        if (m_isAttackButtonDown && m_attackController != null && m_attackController.CanAttack)
        {
            ExecuteAttack();
        }

        UpdateBestTarget();

        UpdateInteractionLabelPosition();
        UpdateAnimation();
    }

    public override void _UnhandledInput(InputEvent p_event)
    {
        if (m_currentState == PlayerState.Dead) return;

        if (p_event.IsActionPressed(ACTION_ATTACK))
        {
            m_isAttackButtonDown = true;
        }
        else if (p_event.IsActionReleased(ACTION_ATTACK))
        {
            m_isAttackButtonDown = false;
        }

        if (m_currentState == PlayerState.Interacting || m_currentState == PlayerState.Attacking || m_currentState == PlayerState.Dashing) return;

        if (p_event.IsActionPressed(ACTION_INTERACT) && m_bestTarget != null)
        {
            ExecuteInteraction();
        }
        else if (p_event.IsActionPressed(ACTION_DASH) && m_movementController != null && !m_movementController.IsDashing)
        {
            ExecuteDash();
        }
    }

    private void UpdateDashUI()
    {
        if (m_dashLabel == null || m_movementController == null) return;

        if (m_movementController.TimeSinceLastDash >= m_movementController.DashCooldown)
        {
            if (m_isDashReadyUI) return;

            m_dashLabel.Text = "Dash: Prêt";
            m_isDashReadyUI = true;
            m_lastDashDeciseconds = -1;
            return;
        }

        m_isDashReadyUI = false;
        int remainingDeciseconds = (int)Math.Ceiling((m_movementController.DashCooldown - m_movementController.TimeSinceLastDash) * 10f);

        if (remainingDeciseconds == m_lastDashDeciseconds) return;

        m_dashLabel.Text = $"Dash: {remainingDeciseconds / 10f:F1}s";
        m_lastDashDeciseconds = remainingDeciseconds;
    }

    private void ExecuteDash()
    {
        if (m_movementController == null) return;

        Vector2 direction = Input.GetVector(ACTION_MOVE_LEFT, ACTION_MOVE_RIGHT, ACTION_MOVE_UP, ACTION_MOVE_DOWN);
        Vector2 dashDirection = direction != Vector2.Zero ? direction.Normalized() : ((m_sprite != null && m_sprite.FlipH) ? Vector2.Left : Vector2.Right);

        if (m_movementController.TryDash(dashDirection))
        {
            SetState(PlayerState.Dashing);
            UpdateDashUI(); // Force update label immediately
        }
    }

    private void ApplyMovement()
    {
        Vector2 direction = Input.GetVector(ACTION_MOVE_LEFT, ACTION_MOVE_RIGHT, ACTION_MOVE_UP, ACTION_MOVE_DOWN);

        if (direction != Vector2.Zero)
        {
            m_currentState = PlayerState.Moving;

            if (m_sprite != null && m_currentState != PlayerState.Attacking)
            {
                m_sprite.FlipH = direction.X < 0;
            }
        }
        else
        {
            m_currentState = PlayerState.Idle;
        }

        if (m_movementController != null)
        {
            m_movementController.Move(direction);
        }
        else
        {
            // Fallback
            float baseSpeed = Stats?.BaseSpeedValue ?? 300f;
            float speedStat = Stats?.GetCurrentValue(StatType.Speed) ?? 0f;
            float finalSpeed = baseSpeed * (1f + (speedStat * 0.05f));
            Velocity = direction * finalSpeed;
            MoveAndSlide();
        }
    }

    private void UpdateInteractionLabelPosition()
    {
        if (m_interactionLabel != null && m_interactionLabel.Visible)
        {
            var viewport = GetViewport();
            if (viewport != null && m_interactionLabel.GetParent() is CanvasLayer)
            {
                var screenPos = GetGlobalTransformWithCanvas().Origin;
                m_interactionLabel.SetGlobalPosition(new Godot.Vector2(screenPos.X - m_interactionLabel.Size.X / 2, screenPos.Y - 80));
            }
        }
    }

    private void UpdateBestTarget()
    {
        m_bestTarget = m_interactionService.GetBestInteractable(GlobalPosition.X, GlobalPosition.Y, m_nearbyInteractables);

        if (m_interactionLabel != null)
        {
            if (m_bestTarget != null)
            {
                m_interactionLabel.Text = m_bestTarget.InteractionPrompt;
                m_interactionLabel.Visible = true;
            }
            else
            {
                m_interactionLabel.Visible = false;
            }
        }
    }

    private async void ExecuteInteraction()
    {
        if (m_bestTarget == null) return;

        SetState(PlayerState.Interacting);
        m_bestTarget.Interact();

        if (m_animationPlayer != null && m_animationPlayer.HasAnimation(m_animInteract))
        {
            if (!m_animationPlayer.IsPlaying() || m_animationPlayer.CurrentAnimation != m_animInteract)
                m_animationPlayer.Play(m_animInteract);
            await ToSignal(m_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }
        else
        {
            await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
        }

        SetState(PlayerState.Idle);
    }

    private void ExecuteAttack()
    {
        if (m_attackController == null) return;

        string direction = (m_sprite != null && m_sprite.FlipH) ? "Left" : "Right";

        if (m_attackController.TryAttack(direction))
        {
            SetState(PlayerState.Attacking);
        }
    }

    private void UpdateAnimation()
    {
        if (m_animationPlayer == null) return;

        if (m_currentState == PlayerState.Attacking) return; // Handled separately

        switch (m_currentState)
        {
            case PlayerState.Idle:
                if ((!m_animationPlayer.IsPlaying() || m_animationPlayer.CurrentAnimation != m_animIdle) && m_animationPlayer.HasAnimation(m_animIdle))
                    m_animationPlayer.Play(m_animIdle);
                break;
            case PlayerState.Moving:
                if ((!m_animationPlayer.IsPlaying() || m_animationPlayer.CurrentAnimation != m_animRun) && m_animationPlayer.HasAnimation(m_animRun))
                    m_animationPlayer.Play(m_animRun);
                break;
        }
    }

    public void SetState(PlayerState p_newState)
    {
        m_currentState = p_newState;
    }

    private void UpdateLevelLabel()
    {
        if (m_levelLabel != null)
        {
            var statTracker = IslandSurvivor.Globals.ServiceRegistry.Instance.StatTracker;
            float level = statTracker.GetCurrentValue(StatType.Level);
            float currentXp = statTracker.GetCurrentValue(StatType.Experience);
            float requiredXp = statTracker.CalculateRequiredXp((int)level);

            m_levelLabel.Text = $"Niveau {level} ({currentXp} / {requiredXp})";
        }
    }

    private void OnLevelChanged(Core.Events.LevelChangedEvent p_event)
    {
        UpdateLevelLabel();

        if (m_xpGainLabel != null)
        {
            m_xpGainLabel.Text = "LEVEL UP!";
            m_xpGainLabel.Visible = true;
            m_xpGainTimer?.Start();
            AudioManager.Instance?.PlaySound2D("Level_Up", GlobalPosition);
        }
    }

    private void OnExperienceGained(Core.Events.ExperienceGainedEvent p_event)
    {
        UpdateLevelLabel();

        if (m_xpGainLabel != null)
        {
            m_xpGainLabel.Text = $"+{p_event.Amount} XP";
            m_xpGainLabel.Visible = true;
            m_xpGainTimer?.Start();
        }
    }

    private void OnXpTimerTimeout()
    {
        if (m_xpGainLabel != null)
        {
            m_xpGainLabel.Visible = false;
        }
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (m_currentState == PlayerState.Dead) return;
        if (m_movementController != null && m_movementController.IsDashing) return; // Invincible during dash

        if (Stats == null) return;

        float currentHealth = Stats.GetCurrentValue(StatType.Health);
        if (currentHealth <= 0) return;

        Stats.ModifyCurrentValue(StatType.Health, -p_amount);

        currentHealth = Stats.GetCurrentValue(StatType.Health);

        if (currentHealth <= 0)
        {
            HandleDeath();
            return;
        }

        this.PlayHitFlash();
        this.PlayShake();

        // Play hurt sound
        if (HurtSound != null)
            AudioManager.Instance?.PlaySound2D(HurtSound, GlobalPosition, p_volumeLinear: HurtVolume);
        else if (!string.IsNullOrEmpty(HurtSoundKey))
            AudioManager.Instance?.PlaySound(HurtSoundKey, p_volumeLinear: HurtVolume);

        // Lightweight camera shake
        Camera2D camera = GetNodeOrNull<Camera2D>("Camera2D");
        if (camera != null)
        {
            camera.PlayShake(0.2f, 8.0f);
        }
    }

    private void HandleDeath()
    {
        SetState(PlayerState.Dead);
        Velocity = Vector2.Zero;

        // Play death sound
        if (DeathSound != null)
            AudioManager.Instance?.PlaySound2D(DeathSound, GlobalPosition, p_volumeLinear: DeathVolume);
        else if (!string.IsNullOrEmpty(DeathSoundKey))
            AudioManager.Instance?.PlaySound(DeathSoundKey, p_volumeLinear: DeathVolume);

        if (m_animationPlayer != null && m_animationPlayer.HasAnimation(m_animDeath))
        {
            m_animationPlayer.Play(m_animDeath);
        }
        else if (m_animationPlayer != null)
        {
            m_animationPlayer.Stop();
        }

        // Emit PlayerDiedEvent
        if (IslandSurvivor.Globals.ServiceRegistry.Instance != null && IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus != null)
        {
            IslandSurvivor.Globals.ServiceRegistry.Instance.EventBus.Publish(new Core.Events.PlayerDiedEvent());
        }
    }

    private void OnInteractionAreaEntered(Area2D p_area)
    {
        GD.Print("PHYSIQUE : Collision détectée avec : " + p_area.Name);
        IInteractable? interactable = p_area as IInteractable ?? p_area.GetParent() as IInteractable;

        if (interactable != null && !m_nearbyInteractables.Contains(interactable))
        {
            m_nearbyInteractables.Add(interactable);
        }
    }

    private void OnInteractionAreaExited(Area2D p_area)
    {
        IInteractable? interactable = p_area as IInteractable ?? p_area.GetParent() as IInteractable;
        if (interactable != null)
        {
            m_nearbyInteractables.Remove(interactable);
        }
    }
}