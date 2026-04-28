using Godot;
using System;
using System.Collections.Generic;
using Core.Interfaces.Spawning;

namespace IslandSurvivor.Nodes.Zones;

/// <summary>
/// [Gameplay][Map][Spawning][Procedural]
/// Defines a zone where resources are randomly spawned within a Polygon2D area.
/// Supports respawning, water validation, and minimum distance between resources.
/// </summary>
public partial class ResourceZone : Node2D, IResourcePopulator
{
    [Export] public int ResourceCount { get; set; } = 10;
    [Export] public Polygon2D SpawningArea { get; set; }

    /// <summary>
    /// List of resource scenes (.tscn) that can be spawned in this zone.
    /// Drag and drop scenes from the FileSystem into this list in the Inspector.
    /// </summary>
    [Export] public Godot.Collections.Array<PackedScene> ResourceScenes { get; set; } = new();

    /// <summary>
    /// Radius around SafeZoneCenter where no resources will spawn.
    /// </summary>
    [Export] public float SafeZoneRadius { get; set; } = 150f;

    /// <summary>
    /// Center of the safe zone, in local coordinates.
    /// </summary>
    [Export] public Vector2 SafeZoneCenter { get; set; } = Vector2.Zero;

    /// <summary>
    /// Minimum distance between spawned resources in this zone.
    /// </summary>
    [Export] public float MinDistanceBetweenResources { get; set; } = 50f;

    /// <summary>
    /// Delay in seconds before attempting to respawn a missing resource.
    /// </summary>
    [Export] public float RespawnInterval { get; set; } = 30f;

    /// <summary>
    /// TileMapLayer used to check for water. Resources won't spawn on tiles present in this layer.
    /// </summary>
    [Export] public TileMapLayer WaterTileMap { get; set; }

    private readonly List<Node2D> m_activeResources = new();
    private float m_respawnTimer = 0f;
    private Random m_random = new();
    private Rect2 m_cachedBounds;

    public override void _Ready()
    {
        YSortEnabled = true;

        if (SpawningArea == null)
        {
            SpawningArea = GetNodeOrNull<Polygon2D>("SpawningArea3");
        }

        if (SpawningArea == null || SpawningArea.Polygon.Length < 3)
        {
            GD.PushWarning($"[ResourceZone] SpawningArea is not assigned or invalid for {Name}.");
            return;
        }

        m_cachedBounds = GetPolygonBounds(SpawningArea.Polygon);
        Populate();
    }

    public override void _Process(double p_delta)
    {
        if (SpawningArea == null || SpawningArea.Polygon.Length < 3) return;

        // Handle respawning
        m_respawnTimer += (float)p_delta;
        if (m_respawnTimer >= RespawnInterval)
        {
            m_respawnTimer = 0f;
            CleanupDestroyedResources();

            // Attempt to spawn multiple missing resources if necessary,
            // but limit to one successful spawn per interval to spread performance cost
            if (m_activeResources.Count < ResourceCount)
            {
                TrySpawnOne();
            }
        }
    }

    public void Populate()
    {
        if (ResourceScenes == null || ResourceScenes.Count == 0)
        {
             GD.PushWarning($"[ResourceZone] No ResourceScenes assigned for {Name}.");
             return;
        }

        CleanupDestroyedResources();
        int toSpawn = ResourceCount - m_activeResources.Count;
        int spawned = 0;
        int maxAttempts = toSpawn * 20;
        int attempts = 0;

        while (spawned < toSpawn && attempts < maxAttempts)
        {
            attempts++;
            if (TrySpawnOne())
            {
                spawned++;
            }
        }

        GD.Print($"[ResourceZone][Gameplay] {Name} populated with {spawned} resources.");
    }

    private bool TrySpawnOne()
    {
        if (ResourceScenes == null || ResourceScenes.Count == 0) return false;

        Vector2 randomPoint = new Vector2(
            (float)m_random.NextDouble() * m_cachedBounds.Size.X + m_cachedBounds.Position.X,
            (float)m_random.NextDouble() * m_cachedBounds.Size.Y + m_cachedBounds.Position.Y
        );

        // Validation 1: Inside Polygon (SpawningArea local space)
        if (!Geometry2D.IsPointInPolygon(randomPoint, SpawningArea.Polygon)) return false;

        // Convert to local ResourceZone coordinates for further checks
        Vector2 localPos = ToLocal(SpawningArea.ToGlobal(randomPoint));

        // Validation 2: Safe Zone
        if (localPos.DistanceTo(SafeZoneCenter) < SafeZoneRadius) return false;

        // Validation 3: Water (Global coordinates for TileMap check)
        if (WaterTileMap != null)
        {
            Vector2 globalPos = ToGlobal(localPos);
            Vector2I tilePos = WaterTileMap.LocalToMap(WaterTileMap.ToLocal(globalPos));
            if (WaterTileMap.GetCellSourceId(tilePos) != -1) return false;
        }

        // Validation 4: Minimum Distance to other resources in the same zone
        foreach (var existing in m_activeResources)
        {
            if (IsInstanceValid(existing) && localPos.DistanceTo(existing.Position) < MinDistanceBetweenResources)
            {
                return false;
            }
        }

        SpawnResource(ResourceScenes[m_random.Next(ResourceScenes.Count)], localPos);
        return true;
    }

    private void SpawnResource(PackedScene p_scene, Vector2 p_localPos)
    {
        if (p_scene == null) return;

        Node instance = p_scene.Instantiate();
        if (instance is Node2D resourceNode)
        {
            AddChild(resourceNode);
            resourceNode.Position = p_localPos;
            resourceNode.Visible = true;
            resourceNode.YSortEnabled = true;

            // Ensure it's on layer 5 (Ressource) - bit 4 (2^4 = 16)
            if (resourceNode is CollisionObject2D collisionObject)
            {
                collisionObject.CollisionLayer = 16;
            }
            // Some resources might have an Area2D/StaticBody2D as a child
            else
            {
                foreach (var child in resourceNode.FindChildren("*", "CollisionObject2D", true))
                {
                    if (child is CollisionObject2D childCO)
                    {
                        childCO.CollisionLayer = 16;
                    }
                }
            }

            m_activeResources.Add(resourceNode);
        }
        else
        {
            instance.QueueFree();
        }
    }

    private void CleanupDestroyedResources()
    {
        m_activeResources.RemoveAll(r => !IsInstanceValid(r));
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
