using Godot;
using System;
using System.Collections.Generic;
using Core.Interfaces;

namespace IslandSurvivor.Nodes;

public partial class ResourceZone : Node2D, IResourcePopulator
{
    [Export] public int ResourceCount { get; set; } = 10;
    [Export] public Polygon2D SpawningArea { get; set; } = null!;

    [Export] public Godot.Collections.Array<PackedScene> ResourceScenes { get; set; } = new();

    [Export] public float SafeZoneRadius { get; set; } = 150f;
    [Export] public Vector2 SafeZoneCenter { get; set; } = Vector2.Zero;

    [Export] public float MinDistanceBetweenResources { get; set; } = 50f;

    [Export] public float RespawnInterval { get; set; } = 30f;

    [Export] public TileMapLayer WaterTileMap { get; set; } = null!;

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

        m_respawnTimer += (float)p_delta;
        if (m_respawnTimer >= RespawnInterval)
        {
            m_respawnTimer = 0f;
            CleanupDestroyedResources();

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

        Vector2 randomPoint = GenerateRandomPointInBounds();

        if (!IsInsideSpawningPolygon(randomPoint)) return false;

        Vector2 localPos = ToLocal(SpawningArea.ToGlobal(randomPoint));

        if (IsInSafeZone(localPos)) return false;
        if (IsOnWater(localPos)) return false;
        if (IsTooCloseToOtherResources(localPos)) return false;

        SpawnResource(ResourceScenes[m_random.Next(ResourceScenes.Count)], localPos);
        return true;
    }

    private Vector2 GenerateRandomPointInBounds()
    {
        return new Vector2(
            (float)m_random.NextDouble() * m_cachedBounds.Size.X + m_cachedBounds.Position.X,
            (float)m_random.NextDouble() * m_cachedBounds.Size.Y + m_cachedBounds.Position.Y
        );
    }

    private bool IsInsideSpawningPolygon(Vector2 p_point)
    {
        return Geometry2D.IsPointInPolygon(p_point, SpawningArea.Polygon);
    }

    private bool IsInSafeZone(Vector2 p_localPos)
    {
        return p_localPos.DistanceSquaredTo(SafeZoneCenter) < SafeZoneRadius * SafeZoneRadius;
    }

    private bool IsOnWater(Vector2 p_localPos)
    {
        if (WaterTileMap == null) return false;

        Vector2 globalPos = ToGlobal(p_localPos);
        Vector2I tilePos = WaterTileMap.LocalToMap(WaterTileMap.ToLocal(globalPos));
        return WaterTileMap.GetCellSourceId(tilePos) != -1;
    }

    private bool IsTooCloseToOtherResources(Vector2 p_localPos)
    {
        float minDistanceSquared = MinDistanceBetweenResources * MinDistanceBetweenResources;
        foreach (var existing in m_activeResources)
        {
            if (IsInstanceValid(existing) && p_localPos.DistanceSquaredTo(existing.Position) < minDistanceSquared)
            {
                return true;
            }
        }
        return false;
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

            if (resourceNode is CollisionObject2D collisionObject)
            {
                collisionObject.CollisionLayer = 16;
            }
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
