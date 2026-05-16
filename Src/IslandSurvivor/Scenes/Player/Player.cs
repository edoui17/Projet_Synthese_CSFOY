using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Extensions;
using IslandSurvivor.Globals;
using IslandSurvivor.Enums;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Managers;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using IslandSurvivor.Nodes.Movement;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, IDamageable
{
    [Export] public StatManager? Stats { get; set; }

    private PlayerState m_currentState = PlayerState.Idle;

    [Export] private AnimationPlayer? m_animationPlayer;
    [Export] private Sprite2D? m_sprite;
    [Export] private Label? m_interactionLabel;
    [Export] private Label? m_debugLabel;
    [Export] private Label? m_levelLabel;
    [Export] private Label? m_xpGainLabel;
    [Export] private Label? m_dashLabel;
    private Timer? m_xpGainTimer;
    [Export] private Area2D? m_interactionArea;

    [ExportGroup("Attack")]
    [Export] private Area2D? m_weaponAreaRight;
    [Export] private Area2D? m_weaponAreaLeft;

    private IslandSurvivor.Nodes.Combat.AttackController? m_attackController;

    private readonly List<IInteractable> m_nearbyInteractables = new();
    private IInteractable? m_bestTarget;
    private readonly IInteractionService m_interactionService = new InteractionService();
    private MovementController? m_movementController;

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

        m_attackController = GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = EntityFaction.Player;
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
        AudioStream swingStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/weapon_swing.wav");
        if (swingStream != null)
        {
            AudioManager.Instance?.PlaySound2D(swingStream, GlobalPosition);
        }

        if (m_animationPlayer != null && m_animationPlayer.HasAnimation("ATTACK"))
        {
            m_animationPlayer.Play("ATTACK");
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
        if (m_debugLabel != null)
        {
            m_debugLabel.Text = m_currentState.ToString();
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
        if (Input.IsActionPressed("attack") && m_attackController != null && m_attackController.CanAttack)
        {
            ExecuteAttack();
        }

        UpdateBestTarget();
        UpdateInteractionLabelPosition();
        UpdateAnimation();
    }

    public override void _Input(InputEvent p_event)
    {
        if (m_currentState == PlayerState.Interacting || m_currentState == PlayerState.Attacking || m_currentState == PlayerState.Dashing) return;

        if (p_event.IsActionPressed("interact") && m_bestTarget != null)
        {
            ExecuteInteraction();
        }
        else if (p_event.IsActionPressed("dash") && m_movementController != null && !m_movementController.IsDashing)
        {
            ExecuteDash();
        }
    }

    private void UpdateDashUI()
    {
        if (m_dashLabel != null && m_movementController != null)
        {
            if (m_movementController.TimeSinceLastDash >= m_movementController.DashCooldown)
            {
                m_dashLabel.Text = "Dash: Prêt";
            }
            else
            {
                m_dashLabel.Text = $"Dash: {(m_movementController.DashCooldown - m_movementController.TimeSinceLastDash):F1}s";
            }
        }
    }

    private void ExecuteDash()
    {
        if (m_movementController == null) return;

        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Vector2 dashDirection = direction != Vector2.Zero ? direction.Normalized() : ((m_sprite != null && m_sprite.FlipH) ? Vector2.Left : Vector2.Right);

        if (m_movementController.TryDash(dashDirection))
        {
            SetState(PlayerState.Dashing);
            UpdateDashUI(); // Force update label immediately
        }
    }

    private void ApplyMovement()
    {
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (direction != Vector2.Zero)
        {
            m_currentState = PlayerState.Moving;

            if (m_sprite != null)
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

        if (m_animationPlayer != null && m_animationPlayer.HasAnimation("INTERACT"))
        {
            m_animationPlayer.Play("INTERACT");
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

        switch (m_currentState)
        {
            case PlayerState.Idle:
                m_animationPlayer.Play("IDLE");
                break;
            case PlayerState.Moving:
                m_animationPlayer.Play("RUN");
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
            m_xpGainTimer.Start();
        }
    }

    private void OnExperienceGained(Core.Events.ExperienceGainedEvent p_event)
    {
        UpdateLevelLabel();

        if (m_xpGainLabel != null)
        {
            m_xpGainLabel.Text = $"+{p_event.Amount} XP";
            m_xpGainLabel.Visible = true;
            m_xpGainTimer.Start();
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
        if (m_movementController != null && m_movementController.IsDashing) return; // Invincible during dash

        if (Stats == null) return;

        float currentHealth = Stats.GetCurrentValue(StatType.Health);
        if (currentHealth <= 0) return;

        Stats.ModifyCurrentValue(StatType.Health, -p_amount);

        this.PlayHitFlash();
        this.PlayShake();

        // Play hurt sound
        AudioStream hurtStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/player_hurt.wav");
        if (hurtStream != null)
        {
            AudioManager.Instance?.PlaySound(hurtStream);
        }

        // Lightweight camera shake
        Camera2D camera = GetNodeOrNull<Camera2D>("Camera2D");
        if (camera != null)
        {
            camera.PlayShake(0.2f, 8.0f);
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