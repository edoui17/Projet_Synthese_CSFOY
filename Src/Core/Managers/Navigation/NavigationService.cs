using System;
using System.Collections.Generic;
using Core.Domain.Models;
using Core.Events;
using Core.Interfaces;
using Core.Interfaces.Navigation;

namespace Core.Managers.Navigation;

public class NavigationService : INavigationService
{
    private readonly IEventBus m_eventBus;
    private readonly IShopManager m_shopManager;
    private readonly IInventoryManager m_inventoryManager;
    private readonly Random m_random = new Random();

    private readonly string[] m_biomes = { "Normal", "Rare", "Dangerous" };

    public NavigationService(IEventBus p_eventBus, IShopManager p_shopManager, IInventoryManager p_inventoryManager)
    {
        m_eventBus = p_eventBus;
        m_shopManager = p_shopManager;
        m_inventoryManager = p_inventoryManager;

        m_eventBus.Subscribe<NavigationApprovedEvent>(OnNavigationApproved);
    }

    private void OnNavigationApproved(NavigationApprovedEvent p_event)
    {
        // Internal logic for when navigation is actually approved
    }

    public IReadOnlyList<IslandDestination> GenerateDestinations(int p_count)
    {
        var destinations = new List<IslandDestination>();

        for (int i = 0; i < p_count; i++)
        {
            int difficulty = m_random.Next(1, 10);
            string biome = m_biomes[m_random.Next(m_biomes.Length)];

            int dangerLevel = biome == "Dangerous" ? m_random.Next(5, 10) : m_random.Next(1, 5);
            int resourceCost = difficulty;

            if (biome == "Rare")
            {
                resourceCost += 2;
            }

            if (i == 0)
            {
                resourceCost = 0;
            }

            int[] validLevels = { 1, 2, 3, 5 };
            int randomLevelIndex = m_random.Next(0, validLevels.Length);
            int randomLevel = validLevels[randomLevelIndex];

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

        m_eventBus.Publish(new NavigationRequestedEvent(p_destination));
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
