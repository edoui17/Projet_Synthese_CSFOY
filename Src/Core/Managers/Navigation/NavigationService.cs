using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;
using Core.Events;
using Core.Interfaces;
using Core.Interfaces;

namespace Core.Managers;

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

        // Replaced string biomes with strong enums for IslandDifficulty logic
        // Poor = 0 (Free), Normal = 1 (Low Cost), Hard = 2 (High Cost/High Reward)
        IslandDifficulty[] difficulties = { IslandDifficulty.Poor, IslandDifficulty.Normal, IslandDifficulty.Hard };

        for (int i = 0; i < p_count; i++)
        {
            IslandDifficulty islandDifficulty = difficulties[m_random.Next(difficulties.Length)];

            // When exactly 2 are requested (Poor + Normal/Hard), enforce it
            if (p_count == 2)
            {
                if (i == 0)
                {
                    islandDifficulty = IslandDifficulty.Poor;
                }
                else
                {
                    IslandDifficulty[] paidDifficulties = { IslandDifficulty.Normal, IslandDifficulty.Hard };
                    islandDifficulty = paidDifficulties[m_random.Next(paidDifficulties.Length)];
                }
            }

            // Adjust costs based on difficulty, as requested
            int resourceCost = 0;
            string biome = "Poor";
            int dangerLevel = 1;

            if (islandDifficulty == IslandDifficulty.Normal)
            {
                resourceCost = m_random.Next(2, 5);
                biome = "Normal";
                dangerLevel = m_random.Next(3, 6);
            }
            else if (islandDifficulty == IslandDifficulty.Hard)
            {
                resourceCost = m_random.Next(6, 12);
                biome = "Hard";
                dangerLevel = m_random.Next(7, 10);
            }

            // First one is always free/Poor, or we just rely on the randomization?
            // "remove the testing free island since now poor island will be free"
            // We will just let the Poor difficulty naturally be free.
            if (islandDifficulty == IslandDifficulty.Poor)
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
                Difficulty: (int)islandDifficulty,
                ResourceCost: resourceCost,
                DangerLevel: dangerLevel
            ));
        }

        return destinations.OrderBy(d => d.ResourceCost).ToList().AsReadOnly();
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
