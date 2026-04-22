using Godot;
using System;
using System.Collections.Generic;
using Core.Interfaces.Spawning;
using System.Linq;

namespace IslandSurvivor.Nodes.Zones;

/// <summary>
/// [Gameplay][Map][Spawning][Procedural]
/// Defines a zone where trees are randomly spawned within a Polygon2D area.
/// </summary>
public partial class TreeZone : Node2D, ITreePopulator
{
    [Export] public int TreeCount { get; set; } = 10;
    [Export] public Polygon2D SpawningArea { get; set; }

    /// <summary>
    /// Radius around SafeZoneCenter where no trees will spawn.
    /// </summary>
    [Export] public float SafeZoneRadius { get; set; } = 150f;

    /// <summary>
    /// Center of the safe zone, in local coordinates.
    /// </summary>
    [Export] public Vector2 SafeZoneCenter { get; set; } = Vector2.Zero;

    private const string TREE_RESOURCES_PATH = "res://Scenes/Ressources/Tree";

    public override void _Ready()
    {
        if (SpawningArea == null)
        {
            SpawningArea = GetNodeOrNull<Polygon2D>("SpawningArea");
        }

        if (SpawningArea == null || SpawningArea.Polygon.Length < 3)
        {
            GD.PushWarning("[TreeZone] SpawningArea is not assigned or has fewer than 3 points.");
            return;
        }

        Populate();
    }

    public void Populate()
    {
        GD.Print($"[TreeZone][Gameplay] Starting population of {TreeCount} trees.");

        var scenePaths = LoadTreeScenePaths();
        if (scenePaths.Count == 0)
        {
            GD.PushWarning($"[TreeZone] No tree scenes found in {TREE_RESOURCES_PATH}.");
            return;
        }

        // Pre-load scenes for efficiency
        var treeScenes = new List<PackedScene>();
        foreach (var path in scenePaths)
        {
            var loadedScene = GD.Load<PackedScene>(path);
            if (loadedScene != null) treeScenes.Add(loadedScene);
        }

        if (treeScenes.Count == 0) return;

        var polygon = SpawningArea.Polygon;
        // Calculate bounds in local coordinates relative to the TreeZone
        var localPolygon = polygon.Select(p => ToLocal(SpawningArea.ToGlobal(p))).ToArray();
        Rect2 bounds = GetPolygonBounds(localPolygon);

        int spawnedCount = 0;
        int attempts = 0;
        int maxAttempts = TreeCount * 10;
        Random random = new();

        while (spawnedCount < TreeCount && attempts < maxAttempts)
        {
            attempts++;
            Vector2 randomPoint = new Vector2(
                (float)random.NextDouble() * bounds.Size.X + bounds.Position.X,
                (float)random.NextDouble() * bounds.Size.Y + bounds.Position.Y
            );

            // Geometry2D.IsPointInPolygon expects points in the same coordinate space as the polygon points
            // polygon is in SpawningArea's local space
            if (Geometry2D.IsPointInPolygon(SpawningArea.ToLocal(ToGlobal(randomPoint)), polygon))
            {
                // Check if the point is outside the safe zone
                if (randomPoint.DistanceTo(SafeZoneCenter) >= SafeZoneRadius)
                {
                    SpawnTree(treeScenes[random.Next(treeScenes.Count)], randomPoint);
                    spawnedCount++;
                }
            }
        }

        GD.Print($"[TreeZone][Gameplay] Successfully spawned {spawnedCount} trees after {attempts} attempts.");
    }

    private void SpawnTree(PackedScene p_scene, Vector2 p_localPos)
    {
        Node instance = p_scene.Instantiate();
        if (instance is Node2D treeNode)
        {
            AddChild(treeNode);
            treeNode.Position = p_localPos;
            treeNode.Scale = Vector2.One;
            treeNode.Visible = true;

            // Ensure it's on layer 5 (Ressource)
            // Layer 5 in Godot is bit 4 (2^4 = 16)
            if (treeNode is CollisionObject2D collisionObject)
            {
                collisionObject.CollisionLayer = 16;
            }
        }
        else
        {
            instance.QueueFree();
        }
    }

    private List<string> LoadTreeScenePaths()
    {
        var paths = new List<string>();
        using var dir = DirAccess.Open(TREE_RESOURCES_PATH);
        if (dir == null) return paths;

        foreach (string fileName in dir.GetFiles())
        {
            if (fileName.EndsWith(".tscn"))
            {
                paths.Add($"{TREE_RESOURCES_PATH}/{fileName}");
            }
        }
        return paths;
    }

    private Rect2 GetPolygonBounds(Vector2[] p_points)
    {
        if (p_points.Length == 0) return new Rect2();

        float minX = p_points[0].X;
        float maxX = p_points[0].X;
        float minY = p_points[0].Y;
        float maxY = p_points[0].Y;

        foreach (var p in p_points)
        {
            minX = Mathf.Min(minX, p.X);
            maxX = Mathf.Max(maxX, p.X);
            minY = Mathf.Min(minY, p.Y);
            maxY = Mathf.Max(maxY, p.Y);
        }

        return new Rect2(minX, minY, maxX - minX, maxY - minY);
    }
}
