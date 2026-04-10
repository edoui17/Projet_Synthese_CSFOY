using Godot;
using System;
using Core.Domain.Models;
using IslandSurvivor.Interfaces;

public partial class PortalInteraction : Area2D, IInteractable
{
    private IslandDestination? m_destination = null;

    public bool IsInteractable => m_destination != null;
    public string InteractionPrompt => m_destination != null ? $"Press E to travel to {m_destination.Biome}" : "";

    public float GetDistanceTo(float p_x, float p_y)
    {
        return GlobalPosition.DistanceTo(new Vector2(p_x, p_y));
    }

    public void Interact()
    {
        if (m_destination != null)
        {
            GD.Print($"[PortalInteraction] Emitting NavigationRequested for destination: {m_destination.Biome}");
            SignalManager.Instance.EmitNavigationRequested(this, m_destination);
        }
    }

    public void SetDestination(IslandDestination p_destination)
    {
        m_destination = p_destination;
    }
}
