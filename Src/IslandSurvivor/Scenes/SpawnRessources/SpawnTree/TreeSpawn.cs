using Godot;
using System;
using System.Collections.Generic;
using Core.Interfaces.Utils;
using System.Linq;

namespace IslandSurvivor.Scenes.SpawnRessources.SpawnTree;

/// <summary>
/// [Gameplay][Map][Spawning]
/// Marker node responsible for spawning a random tree at its location.
/// </summary>
public partial class TreeSpawn : Marker2D
{
    private const string TREE_RESOURCES_PATH = "res://Scenes/Ressources/Tree";

    /// <summary>
    /// Spawns a random tree from the resources directory.
    /// </summary>
    /// <param name="p_selector">The random selector to use.</param>
    public void SpawnTree(IRandomSelector<string> p_selector)
    {
        var treeScenes = LoadTreeScenes();

        if (treeScenes.Count == 0)
        {
            GD.PushWarning($"[TreeSpawn] No tree scenes found in {TREE_RESOURCES_PATH}.");
            return;
        }

        string selectedPath = p_selector.SelectRandom(treeScenes);
        if (string.IsNullOrEmpty(selectedPath))
        {
            return;
        }

        PackedScene scene = GD.Load<PackedScene>(selectedPath);
        if (scene == null)
        {
            GD.PushWarning($"[TreeSpawn] Failed to load tree scene at {selectedPath}.");
            return;
        }

        Node instance = scene.Instantiate();
        if (instance is Node2D treeNode)
        {
            AddChild(treeNode);
            // The tree is already at the correct position because it's a child of this Marker2D.
            // Requirement 5: No rotation as requested in clarification.
        }
        else
        {
            instance.QueueFree();
            GD.PushWarning($"[TreeSpawn] Instantiated scene is not a Node2D: {selectedPath}.");
        }
    }

    private List<string> LoadTreeScenes()
    {
        var scenes = new List<string>();

        using var dir = DirAccess.Open(TREE_RESOURCES_PATH);
        if (dir == null)
        {
            GD.PushWarning($"[TreeSpawn] Cannot open directory: {TREE_RESOURCES_PATH}. Error: {DirAccess.GetOpenError()}");
            return scenes;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".tscn"))
            {
                scenes.Add($"{TREE_RESOURCES_PATH}/{fileName}");
            }
            fileName = dir.GetNext();
        }

        return scenes;
    }
}
