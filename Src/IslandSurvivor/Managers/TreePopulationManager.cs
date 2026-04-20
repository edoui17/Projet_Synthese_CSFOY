using Godot;
using System.Linq;
using Core.Utils;
using IslandSurvivor.Scenes.SpawnRessources.SpawnTree;

namespace IslandSurvivor.Managers;

/// <summary>
/// [Gameplay][Map][Spawning][Algorithme]
/// Manager responsible for triggering tree spawning across the map.
/// </summary>
public partial class TreePopulationManager : Node
{
    /// <summary>
    /// Finds all TreeSpawn nodes in the scene and triggers their spawning.
    /// </summary>
    public void PopulateTrees()
    {
        var treeSpawns = GetTreeSpawns(GetTree().Root);
        var selector = new RandomSelector<string>();

        foreach (var spawn in treeSpawns)
        {
            spawn.SpawnTree(selector);
        }

        GD.Print($"[TreePopulationManager] Populated {treeSpawns.Count} tree spawn points.");
    }

    private System.Collections.Generic.List<TreeSpawn> GetTreeSpawns(Node p_node)
    {
        var result = new System.Collections.Generic.List<TreeSpawn>();

        if (p_node is TreeSpawn spawn)
        {
            result.Add(spawn);
        }

        foreach (Node child in p_node.GetChildren())
        {
            result.AddRange(GetTreeSpawns(child));
        }

        return result;
    }
}
