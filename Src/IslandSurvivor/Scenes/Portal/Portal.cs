using Godot;
using System;
using Core.Domain.Models;
using IslandSurvivor.Interfaces;

public partial class Portal : Node2D
{
	private AnimationPlayer m_animationPlayer;
	private PortalInteraction m_interactionArea;

	public override void _Ready()
	{
		m_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		// The InteractionArea child has the script attached now
		m_interactionArea = GetNode<PortalInteraction>("InteractionArea");

		// Auto-activate the portal and point it to the Home Island if we are NOT in the PlayerHub.
		var currentScenePath = GetTree()?.CurrentScene?.SceneFilePath;
		if (currentScenePath != null && !currentScenePath.Contains("PlayerHub"))
		{
			ActivatePortal(IslandDestination.HomeIsland);
		}
	}

	public void ActivatePortal(IslandDestination p_destination)
	{
		m_animationPlayer.Play("ACTIVE");

		if (m_interactionArea != null)
		{
			m_interactionArea.SetDestination(p_destination);
		}
	}
}
