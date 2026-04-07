using Godot;
using System;
using System.Collections.Generic;
using IslandSurvivor.Managers;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Resources;
using IslandSurvivor.Nodes;
using IslandSurvivor.Enums;
using Core.Managers.Stats;

public partial class Player : CharacterBody2D
{
	[Export] public PlayerMovementData? MovementData { get; set; }
	[Export] public StatManager? Stats { get; set; }

	private PlayerState m_currentState = PlayerState.Idle;
	private AnimationPlayer? m_animationPlayer;
	private Sprite2D? m_sprite;
	private Label? m_interactionLabel;
	private Area2D? m_interactionArea;

	private readonly List<IInteractable> m_nearbyInteractables = new();
	private IInteractable? m_bestTarget;
	private readonly IInteractionService m_interactionService = new InteractionService();

	public override void _Ready()
	{
		m_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		m_sprite = GetNode<Sprite2D>("Sprite2D");
		m_interactionLabel = GetNode<Label>("InteractionLabel");
		m_interactionArea = GetNode<Area2D>("PlayerInteraction");

		m_interactionLabel.Visible = false;

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
		if (m_currentState == PlayerState.Interacting)
		{
			Velocity = Vector2.Zero;
			MoveAndSlide();
			return;
		}

		ApplyMovement(p_delta);
		UpdateBestTarget();
		UpdateAnimation();
	}

	public override void _Input(InputEvent p_event)
	{
		if (p_event.IsActionPressed("interact") && m_bestTarget != null && m_currentState != PlayerState.Interacting)
		{
			ExecuteInteraction();
		}
	}

	private void ApplyMovement(double p_delta)
	{
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		float speed = Stats?.GetCurrentValue(StatType.Speed) ?? 300f;
		float accel = MovementData?.Acceleration ?? 2000f;
		float friction = MovementData?.Friction ?? 1500f;

		if (direction != Vector2.Zero)
		{
			Velocity = Velocity.MoveToward(direction * speed, accel * (float)p_delta);
			m_currentState = PlayerState.Moving;

			if (m_sprite != null)
			{
				m_sprite.FlipH = direction.X < 0;
			}
		}
		else
		{
			Velocity = Velocity.MoveToward(Vector2.Zero, friction * (float)p_delta);
			if (Velocity.Length() < 0.1f)
			{
				m_currentState = PlayerState.Idle;
			}
		}

		MoveAndSlide();
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
		if (m_bestTarget == null) return;

		SetState(PlayerState.Interacting);
		m_bestTarget.Interact();

		// Simulate interaction time / animation if needed
		// For now, just wait a bit and return to Idle
		await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

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
			case PlayerState.Interacting:
				m_animationPlayer.Play("INTERACT");
				break;
		}
	}

	public void SetState(PlayerState p_newState)
	{
		m_currentState = p_newState;
	}

	private void OnInteractionAreaEntered(Area2D p_area)
	{
		if (p_area is IInteractable interactable)
		{
			m_nearbyInteractables.Add(interactable);
		}
	}

	private void OnInteractionAreaExited(Area2D p_area)
	{
		if (p_area is IInteractable interactable)
		{
			m_nearbyInteractables.Remove(interactable);
		}
	}
}
