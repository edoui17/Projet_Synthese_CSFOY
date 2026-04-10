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

        // Guarantee at least one free, low-tier island
        destinations.Add(new IslandDestination(
            Id: $"island_{Guid.NewGuid().ToString().Substring(0, 8)}",
            ScenePath: "res://Scenes/GameMap/game_map.tscn",
            Biome: "Normal",
            Difficulty: 1,
            ResourceCost: 0,
            DangerLevel: 1
        ));

        for (int i = 1; i < p_count; i++)
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

            destinations.Add(new IslandDestination(
                Id: $"island_{Guid.NewGuid().ToString().Substring(0, 8)}",
                ScenePath: "res://Scenes/GameMap/game_map.tscn", // The target dynamic island map scene
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
            m_signalManager.EmitNavigationRequested(this, p_destination);
            return true;
        }

        // For this US, cost is 1 of each resource (as defined in the user's interaction script previously),
        // but we'll use the destination's cost to demonstrate we can use it. The problem says "cost and danger at 0" for HomeIsland.
        // We will assume 1 of each (Viande, Bois, Roche, Or) if cost > 0 or whatever cost logic needed.
        // The previous InteractionScript had cost = 1. We'll use the ResourceCost value.

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

            // Deduct
            // In the Godot InteractionScript, it emits "ResourceSpent", which the InventoryNode handles.
            // Since we are in the Core logic, we can directly modify InventoryManager AND emit the event.
            // But we should follow the same pattern:
            p_inventoryManager.RemoveMaterial("Viande", requiredCost);
            m_signalManager.EmitResourceSpent(this, "Viande", requiredCost);

            p_inventoryManager.RemoveMaterial("Bois", requiredCost);
            m_signalManager.EmitResourceSpent(this, "Bois", requiredCost);

            p_inventoryManager.RemoveMaterial("Roche", requiredCost);
            m_signalManager.EmitResourceSpent(this, "Roche", requiredCost);

            p_inventoryManager.RemoveMaterial("Or", requiredCost);
            m_signalManager.EmitResourceSpent(this, "Or", requiredCost);
        }

        // Do not emit NavigationRequested here anymore. Wait for the player to interact with the Portal.
        // We will just return true indicating the purchase was successful.
        return true;
    }
}
