using Godot;
using System;
using System.Collections.Generic;
using Core.Domain.Models;
using Core.Interfaces.Navigation;
using Core.Managers.Navigation;

namespace IslandSurvivor.Scenes.NavigationMenu;

public partial class NavigationMenu : Control
{
    private VBoxContainer m_destinationsContainer;
    private INavigationService m_navigationService;
    private IReadOnlyList<IslandDestination> m_currentOptions;

    public override void _Ready()
    {
        m_navigationService = new NavigationService(SignalManager.Instance);

        m_destinationsContainer = GetNode<VBoxContainer>("PanelContainer/VBoxContainer/DestinationsContainer");

        // Hide by default
        Visible = false;
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

        // Add "Home Hub" option
        var homeBtn = new Button();
        homeBtn.Text = "Return to Home Hub (Cost: Free)";
        homeBtn.Pressed += () => OnDestinationSelected(IslandDestination.HomeIsland);
        m_destinationsContainer.AddChild(homeBtn);

        // Generate 3 random islands
        m_currentOptions = m_navigationService.GenerateDestinations(3);

        foreach (var destination in m_currentOptions)
        {
            var btn = new Button();
            btn.Text = $"Travel to {destination.Biome} Island (Diff: {destination.Difficulty}, Danger: {destination.DangerLevel}, Cost: {destination.ResourceCost} of each)";

            // Local copy for the closure
            IslandDestination destCopy = destination;
            btn.Pressed += () => OnDestinationSelected(destCopy);

            m_destinationsContainer.AddChild(btn);
        }
    }

    private void OnDestinationSelected(IslandDestination p_destination)
    {
        GD.Print($"[NavigationMenu] Selected destination: {p_destination.Biome} (ID: {p_destination.Id})");

        if (InventoryNode.Instance == null || InventoryNode.Instance.Manager == null)
        {
            GD.PrintErr("[NavigationMenu] Error: InventoryNode or its Manager is missing.");
            return;
        }

        bool success = m_navigationService.TryNavigate(InventoryNode.Instance.Manager, p_destination);

        if (success)
        {
            GD.Print("[NavigationMenu] Navigation successful! Deducted resources. Activating portal...");

            // Activate the portal and set its destination
            var portalNode = GetNodeOrNull<Portal>("../Portal");
            if (portalNode != null)
            {
                portalNode.ActivatePortal(p_destination);
            }
            else
            {
                GD.PrintErr("[NavigationMenu] Portal node not found! Make sure it is a sibling to NavigationMenu.");
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
