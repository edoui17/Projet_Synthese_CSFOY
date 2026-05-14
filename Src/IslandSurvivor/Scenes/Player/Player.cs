using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
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
    private Timer? m_xpGainTimer;
    [Export] private Area2D? m_interactionArea;
    [Export] private Area2D? m_weaponAreaRight;
    [Export] private Area2D? m_weaponAreaLeft;

    private readonly List<IInteractable> m_nearbyInteractables = new();
    private IInteractable? m_bestTarget;
    private readonly IInteractionService m_interactionService = new InteractionService();
    private MovementController? m_movementController;
    private readonly HashSet<IDamageable> m_hitTargetsThisAttack = new();

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

        if (m_weaponAreaRight != null && m_weaponAreaLeft != null)
        {
            m_weaponAreaRight.AreaEntered += OnWeaponAreaEntered;
            m_weaponAreaRight.BodyEntered += OnWeaponBodyEntered;
            m_weaponAreaLeft.AreaEntered += OnWeaponAreaEntered;
            m_weaponAreaLeft.BodyEntered += OnWeaponBodyEntered;

            // Désactivé par défaut
            m_weaponAreaRight.Monitoring = false;
            m_weaponAreaLeft.Monitoring = false;
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_debugLabel != null)
        {
            m_debugLabel.Text = m_currentState.ToString();
        }

        if (m_currentState == PlayerState.Interacting || m_currentState == PlayerState.Attacking)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        ApplyMovement();
        UpdateBestTarget();
        UpdateAnimation();
    }

    public override void _Input(InputEvent p_event)
    {
        if (m_currentState == PlayerState.Interacting || m_currentState == PlayerState.Attacking) return;

        if (p_event.IsActionPressed("interact") && m_bestTarget != null)
        {
            ExecuteInteraction();
        }
        else if (p_event.IsActionPressed("attack"))
        {
            ExecuteAttack();
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
            float baseSpeed = Stats?.BaseSpeed ?? 300f;
            float speedStat = Stats?.GetCurrentValue(StatType.Speed) ?? 0f;
            float finalSpeed = baseSpeed * (1f + (speedStat * 0.05f));
            Velocity = direction * finalSpeed;
            MoveAndSlide();
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

    private async void ExecuteAttack()
    {
        GD.Print("[COMBAT] Attack started!");
        SetState(PlayerState.Attacking);
        m_hitTargetsThisAttack.Clear();

        // Activation de la zone d'arme selon l'orientation
        if (m_sprite != null && m_weaponAreaLeft != null && m_weaponAreaRight != null)
        {
            m_weaponAreaLeft.Monitoring = m_sprite.FlipH;
            m_weaponAreaRight.Monitoring = !m_sprite.FlipH;
        }

        if (m_animationPlayer != null && m_animationPlayer.HasAnimation("ATTACK"))
        {
            m_animationPlayer.Play("ATTACK");
            await ToSignal(m_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }
        else
        {
            GD.Print("[COMBAT] No animation found");
            await ToSignal(GetTree().CreateTimer(0.4f), SceneTreeTimer.SignalName.Timeout);
        }

        if (m_weaponAreaLeft != null) m_weaponAreaLeft.Monitoring = false;
        if (m_weaponAreaRight != null) m_weaponAreaRight.Monitoring = false;

        SetState(PlayerState.Idle);
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
        if (Stats == null) return;

        float currentHealth = Stats.GetCurrentValue(StatType.Health);
        if (currentHealth <= 0) return;

        Stats.ModifyCurrentValue(StatType.Health, -p_amount);
    }

    private void ApplyDamage(IDamageable p_target)
    {
        if (m_hitTargetsThisAttack.Contains(p_target)) return;

        m_hitTargetsThisAttack.Add(p_target);

        float attackStat = Stats?.GetCurrentValue(StatType.Attack) ?? 0f;
        float baseDamage = Stats?.BaseDamage ?? 10f;
        float finalDamageFloat = baseDamage * (1f + (attackStat * 0.05f));
        int finalDamage = Mathf.RoundToInt(finalDamageFloat);

        GD.Print($"[COMBAT] Hit target! Dealing {finalDamage} damage.");
        p_target.TakeDamage(finalDamage, this);
    }

    private void OnWeaponAreaEntered(Area2D p_area)
    {
        if (m_currentState != PlayerState.Attacking) return;

        IDamageable? damageable = p_area as IDamageable ?? p_area.GetParent() as IDamageable;
        if (damageable != null) ApplyDamage(damageable);
    }

    private void OnWeaponBodyEntered(Node2D p_body)
    {
        if (m_currentState != PlayerState.Attacking) return;

        if (p_body is IDamageable damageable) ApplyDamage(damageable);
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