using Godot;
using System;
using Core.Domain.Models;
using IslandSurvivor.Interfaces;

public partial class PortalInteraction : Area2D, IInteractable
{
    private IslandDestination? m_destination = null;

    public bool IsInteractable => m_destination != null;

    public string InteractionPrompt
    {
        get
        {
            if (m_destination != null)
            {
                if (m_destination.Id == IslandDestination.HomeIsland.Id)
                {
                    return "Return Home";
                }
                return $"Travel to {m_destination.Biome}";
            }
            return "";
        }
    }

    public float GetDistanceTo(float p_x, float p_y)
    {
        return GlobalPosition.DistanceTo(new Vector2(p_x, p_y));
    }

    public void Interact()
    {
        if (m_destination != null)
        {
            GD.Print($"[PortalInteraction] Emitting TeleportRequested for destination: {m_destination.Biome}");

            // Visual/Audio Feedback for portal interaction
            AudioStreamPlayer2D ambientAudio = GetParent().GetNodeOrNull<AudioStreamPlayer2D>("AmbientAudio");
            if (ambientAudio != null && !ambientAudio.Playing)
            {
                ambientAudio.Play();
                // Fade in
                ambientAudio.VolumeDb = -40f;
                Tween tween = CreateTween();
                tween.TweenProperty(ambientAudio, "volume_db", 0f, 2.0f);
            }

            // Shader activation logic for the portal material
            Sprite2D portalSprite = GetParent().GetNodeOrNull<Sprite2D>("Sprite2D"); // Assumes Sprite2D is child of Portal parent
            if (portalSprite != null && portalSprite.Material is ShaderMaterial shaderMat)
            {
                // This triggers the shader by modifying a parameter or simply modulating
                Tween tween = CreateTween();
                tween.TweenProperty(portalSprite, "modulate", new Color(2f, 2f, 2f, 1f), 1.0f);
            }

            // Teleport immediately to show loading screen during sync
            SignalManager.Instance.EmitTeleportRequested(this, m_destination);
        }
    }

    public void SetDestination(IslandDestination p_destination)
    {
        m_destination = p_destination;
    }
}
