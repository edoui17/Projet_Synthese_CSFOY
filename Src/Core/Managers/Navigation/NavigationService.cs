namespace Core.Managers.Navigation;

using System;
using System.Collections.Generic;
using Core.Domain.Models;
using Core.Events;
using Core.Interfaces;
using Core.Interfaces.Navigation;

public class NavigationService : INavigationService
{
    private readonly ISignalManager m_signalManager;
    private readonly IEventBus m_eventBus;
    private readonly IShopManager m_shopManager;
    private readonly IInventoryManager m_inventoryManager;
    private readonly Random m_random = new Random();

    private readonly string[] m_biomes = { "Normal", "Rare", "Dangerous" };

    public NavigationService(ISignalManager p_signalManager, IEventBus p_eventBus, IShopManager p_shopManager, IInventoryManager p_inventoryManager)
    {
        m_signalManager = p_signalManager;
        m_eventBus = p_eventBus;
        m_shopManager = p_shopManager;
        m_inventoryManager = p_inventoryManager;

        m_eventBus.Subscribe<NavigationApprovedEvent>(OnNavigationApproved);
    }

    private void OnNavigationApproved(NavigationApprovedEvent p_event)
    {
        // Internal logic for when navigation is actually approved
        // e.g. signaling to the Godot frontend that it's safe to load the scene.
        // Currently handled partially by NavigationManager in Godot listening to signals,
        // so we'll let the event bus bridge this to Godot later if needed.
    }

    public IReadOnlyList<IslandDestination> GenerateDestinations(int p_count)
    {
        var destinations = new List<IslandDestination>();

        // We have 5 levels: Level1, Level2, Level3, Level4, Level5
        // We can pick randomly from them

        for (int i = 0; i < p_count; i++)
        {
            int difficulty = m_random.Next(1, 10);
            string biome = m_biomes[m_random.Next(m_biomes.Length)];

            // Adjust danger and cost based on biome
            int dangerLevel = biome == "Dangerous" ? m_random.Next(5, 10) : m_random.Next(1, 5);
            int resourceCost = difficulty; // For simplicity, cost matches difficulty

            // If it's a rare biome, slightly more cost but much better reward later on
            if (biome == "Rare")
            {
                resourceCost += 2;
            }

            if (i == 0)
            {
                resourceCost = 0;
            }

            int randomLevel = m_random.Next(1, 6);
            string scenePath = $"res://Scenes/Level/Level{randomLevel}/Level{randomLevel}.tscn";

            destinations.Add(new IslandDestination(
                Id: $"island_{Guid.NewGuid().ToString().Substring(0, 8)}",
                ScenePath: scenePath,
                Biome: biome,
                Difficulty: difficulty,
                ResourceCost: resourceCost,
                DangerLevel: dangerLevel
            ));
        }
        return destinations.AsReadOnly();
    }

    public bool TryNavigate(IslandDestination p_destination)
    {
        if (!CanAffordIsland(p_destination))
        {
            return false;
        }

        // Instead of directly coupling with IInventoryManager to deduct items,
        // we publish an event. InventoryManager will subscribe, validate, and emit NavigationApproved/Rejected.
        m_eventBus.Publish(new NavigationRequestedEvent(p_destination));

        // Since we are transitioning to async/queued events, the return value here might need to be removed in future passes.
        // For now, returning true implies the request was successfully dispatched.
        return true;
    }

    public bool CanAffordIsland(IslandDestination p_destination)
    {
        if (p_destination.ResourceCost <= 0 || p_destination.Id == IslandDestination.HomeIsland.Id)
        {
            return true;
        }

        return m_shopManager.CanAffordIsland(m_inventoryManager, p_destination.ResourceCost);
    }
}
