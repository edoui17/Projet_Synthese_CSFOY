namespace Core.Managers.Navigation;

using System;
using System.Collections.Generic;
using Core.Domain.Models;
using Core.Interfaces;
using Core.Interfaces.Navigation;

public class NavigationService : INavigationService
{
    private readonly ISignalManager m_signalManager;
    private readonly Random m_random = new Random();

    private readonly string[] m_biomes = { "Normal", "Rare", "Dangerous" };

    public NavigationService(ISignalManager p_signalManager)
    {
        m_signalManager = p_signalManager;
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

    public bool TryNavigate(IInventoryManager p_inventoryManager, IslandDestination p_destination)
    {
        // HomeIsland is free
        if (p_destination.Id == IslandDestination.HomeIsland.Id)
        {
            // Do not emit here, wait for portal interaction
            return true;
        }

        int requiredCost = p_destination.ResourceCost;

        if (requiredCost > 0)
        {
            bool hasViande = p_inventoryManager.GetMaterialCount("Viande") >= requiredCost;
            bool hasBois = p_inventoryManager.GetMaterialCount("Bois") >= requiredCost;
            bool hasRoche = p_inventoryManager.GetMaterialCount("Roche") >= requiredCost;
            bool hasOr = p_inventoryManager.GetMaterialCount("Or") >= requiredCost;

            if (!hasViande || !hasBois || !hasRoche || !hasOr)
            {
                return false;
            }

            p_inventoryManager.RemoveMaterial("Viande", requiredCost);
            m_signalManager.EmitResourceSpent(this, "Viande", requiredCost);

            p_inventoryManager.RemoveMaterial("Bois", requiredCost);
            m_signalManager.EmitResourceSpent(this, "Bois", requiredCost);

            p_inventoryManager.RemoveMaterial("Roche", requiredCost);
            m_signalManager.EmitResourceSpent(this, "Roche", requiredCost);

            p_inventoryManager.RemoveMaterial("Or", requiredCost);
            m_signalManager.EmitResourceSpent(this, "Or", requiredCost);
        }

        return true;
    }
}
