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

public partial class Player : CharacterBody2D
{
	[Export] public StatManager? Stats { get; set; }

	private PlayerState m_currentState = PlayerState.Idle;

    [Export]  private AnimationPlayer? m_animationPlayer;
    [Export]  private Sprite2D? m_sprite;
    [Export]  private Label? m_interactionLabel;
    [Export]  private Label? m_debugLabel;
    [Export]  private Area2D? m_interactionArea;
    [Export]  private Area2D? m_weaponArea;

	private readonly List<IInteractable> m_nearbyInteractables = new();
	private IInteractable? m_bestTarget;
	private readonly IInteractionService m_interactionService = new InteractionService();
	private MovementController? m_movementController;

	public override void _Ready()
	{
		m_interactionLabel.Visible = false;

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
			float speed = Stats?.GetCurrentValue(StatType.Speed) ?? 300f;
			Velocity = direction * speed;
			MoveAndSlide();
		}
	}

	private void UpdateBestTarget()
	{
		m_bestTarget = m_interactionService.GetBestInteractable(GlobalPosition.X, GlobalPosition.Y, m_nearbyInteractables);

		if (m_bestTarget != null && m_interactionLabel != null)
		{
			m_interactionLabel.Text = m_bestTarget.InteractionPrompt;
			m_interactionLabel.Visible = true;
		}
		else if (m_interactionLabel != null)
		{
			m_interactionLabel.Visible = false;
		}
	}

	private async void ExecuteInteraction()
	{
    GD.Print(m_bestTarget == null);
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
			// Fallback if animation is missing
			await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
		}

		SetState(PlayerState.Idle);
	}

	private async void ExecuteAttack()
	{
		SetState(PlayerState.Attacking);

        if (m_weaponArea != null)
        {
            // Execute AoE Damage immediately
            int attackDamage = (int)(Stats?.GetCurrentValue(StatType.Attack) ?? 10f); // Default to 10 if missing

            var hitTargets = new HashSet<IDamageable>();

            var overlappingAreas = m_weaponArea.GetOverlappingAreas();
            foreach (var area in overlappingAreas)
            {
                if (area is IDamageable damageable)
                {
                    hitTargets.Add(damageable);
                }
                else if (area.GetParent() is IDamageable parentDamageable)
                {
                    hitTargets.Add(parentDamageable);
                }
            }

            var overlappingBodies = m_weaponArea.GetOverlappingBodies();
            foreach (var body in overlappingBodies)
            {
                if (body is IDamageable damageable)
                {
                    hitTargets.Add(damageable);
                }
            }

            foreach (var target in hitTargets)
            {
                target.TakeDamage(attackDamage, this);
            }
        }

		if (m_animationPlayer != null && m_animationPlayer.HasAnimation("ATTACK"))
		{
			m_animationPlayer.Play("ATTACK");
			await ToSignal(m_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
		}
		else
		{
			GD.Print("[COMBAT] Attack triggered (no animation found)");
			await ToSignal(GetTree().CreateTimer(0.4f), SceneTreeTimer.SignalName.Timeout);
		}

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

  private void OnInteractionAreaEntered(Area2D p_area)
  {
    // LOG DE DEBUG : Si ce message s'affiche, la collision fonctionne !
    GD.Print("PHYSIQUE : Collision détectée avec le nœud : " + p_area.Name);

    IInteractable interactable = p_area as IInteractable ?? p_area.GetParent() as IInteractable;

    if (interactable != null)
    {
      GD.Print("LOGIQUE : IInteractable trouvé sur " + p_area.GetParent().Name);
      if (!m_nearbyInteractables.Contains(interactable))
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
