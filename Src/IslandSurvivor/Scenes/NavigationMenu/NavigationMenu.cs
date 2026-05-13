using Godot;
using System;
using System.Collections.Generic;
using Core.Domain.Models;
using Core.Interfaces.Navigation;
using Core.Managers.Navigation;

namespace IslandSurvivor.Scenes.NavigationMenu;

public partial class NavigationMenu : Control
{
    private VBoxContainer m_destinationsContainer = null!;
    private INavigationService m_navigationService = null!;
    private IReadOnlyList<IslandDestination> m_currentOptions = null!;

    private Node2D? FindPlayer(Node? parent)
    {
        if (parent == null) return null;
        if (parent is Node2D node && node.Name == "Player") return node;
        foreach (Node child in parent.GetChildren())
        {
            var result = FindPlayer(child);
            if (result != null) return result;
        }
        return null;
    }

    public override void _Ready()
    {
        m_navigationService = Globals.ServiceRegistry.Instance.NavigationService;

        m_destinationsContainer = GetNode<VBoxContainer>("PanelContainer/VBoxContainer/DestinationsContainer");

        // Hide by default
        Visible = false;
    }

    private bool IsPlayerHome()
    {
        var scoreManager = GetTree().CurrentScene.GetNodeOrNull<Nodes.StatsManager.ScoreManager>("ScoreManager");
        if (scoreManager != null)
        {
            var tracker = scoreManager.GetTracker();
            return tracker.GetSessionState().CurrentIslandId == IslandDestination.HomeIsland.Id;
        }
        // Fallback check based on scene name if ScoreManager is absent
        return GetTree().CurrentScene.SceneFilePath.Contains("PlayerHub");
    }

    public void OpenMenu()
    {
        Visible = true;
        GenerateOptions();
    }

    public void CloseMenu()
    {
        Visible = false;
    }

    private void GenerateOptions()
    {
        // Clear previous options
        foreach (Node child in m_destinationsContainer.GetChildren())
        {
            child.QueueFree();
        }

        bool isHome = IsPlayerHome();

        if (isHome)
        {
            // Generate 5 random islands
            m_currentOptions = m_navigationService.GenerateDestinations(5);

            foreach (var destination in m_currentOptions)
            {
                var btn = new Button();
                if (destination.ResourceCost == 0)
                {
                    btn.Text = $"Test Island {destination.Id.Substring(0, 5)} (Cost: 0, Type: {destination.Biome})";
                }
                else
                {
                    btn.Text = $"Island {destination.Id.Substring(0, 5)} (Cost: {destination.ResourceCost}, Type: {destination.Biome})";
                }

                // Local copy for the closure
                IslandDestination destCopy = destination;
                btn.Pressed += () => OnDestinationSelected(destCopy);

                m_destinationsContainer.AddChild(btn);
            }
        }
        else
        {
            // Add "Return Home" option
            var homeBtn = new Button();
            homeBtn.Text = "Return Home";
            homeBtn.Pressed += () => OnDestinationSelected(IslandDestination.HomeIsland);
            m_destinationsContainer.AddChild(homeBtn);
        }
    }

    private void OnDestinationSelected(IslandDestination p_destination)
    {
        GD.Print($"[NavigationMenu] Selected destination: {p_destination.Biome} (ID: {p_destination.Id})");

        bool success = m_navigationService.TryNavigate(p_destination);

        if (success)
        {
            GD.Print("[NavigationMenu] Navigation successful! Deducted resources. Activating portal...");

            // Activate the portal and set its destination
            var portalNode = GetTree().CurrentScene.GetNodeOrNull<Portal>("Portal");
            if (portalNode != null)
            {
                if (IsPlayerHome())
                {
                    // No movement on home
                }
                else
                {
                    var playerNode = FindPlayer(GetTree().CurrentScene);
                    if (playerNode != null)
                    {
                        // Portal moves near player
                        portalNode.GlobalPosition = playerNode.GlobalPosition;
                    }
                }

                portalNode.ActivatePortal(p_destination);
            }
            else
            {
                GD.PrintErr("[NavigationMenu] Portal node not found! Make sure it is at Main/Portal.");
            }

            CloseMenu();
        }
        else
        {
            GD.Print("[NavigationMenu] Not enough resources to travel!");
        }
    }

    public void _on_close_btn_pressed()
    {
        CloseMenu();
    }
}
