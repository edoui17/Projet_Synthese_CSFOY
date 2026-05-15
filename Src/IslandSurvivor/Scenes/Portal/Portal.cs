using Godot;
using System;
using Core.Domain.Models;
using IslandSurvivor.Interfaces;

public partial class Portal : Node2D
{
    // On ajoute une référence pour le nœud audio
    private AudioStreamPlayer2D m_ambientAudio;
    private AnimationPlayer m_animationPlayer = null!;
    private PortalInteraction m_interactionArea = null!;

    public override void _Ready()
    {
        m_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        m_interactionArea = GetNode<PortalInteraction>("InteractionArea");

        // Initialisation de la référence audio
        m_ambientAudio = GetNode<AudioStreamPlayer2D>("AmbientAudio");

        var currentScenePath = GetTree()?.CurrentScene?.SceneFilePath;
        if (currentScenePath != null && !currentScenePath.Contains("PlayerHub"))
        {
            ActivatePortal(IslandDestination.HomeIsland);
        }
    }

    public void ActivatePortal(IslandDestination p_destination)
    {
        // 1. Déclenche l'animation visuelle
        m_animationPlayer.Play("ACTIVE");

        // 2. Déclenche le son seulement si le portail devient vraiment actif
        if (m_ambientAudio != null && !m_ambientAudio.Playing)
        {
            m_ambientAudio.Play();
            GD.Print("[Portal] Activation du son d'ambiance.");
        }

        // 3. Configure la destination de l'interaction
        if (m_interactionArea != null)
        {
            m_interactionArea.SetDestination(p_destination);
        }
    }

    // Optionnel : Arrêter le son si le portail se désactive
    public void DeactivatePortal()
    {
        m_animationPlayer.Play("IDLE"); // Remplace par ton animation de repos

        if (m_ambientAudio != null && m_ambientAudio.Playing)
        {
            m_ambientAudio.Stop();
            GD.Print("[Portal] Son d'ambiance arrêté.");
        }
    }
}