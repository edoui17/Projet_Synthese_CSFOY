using Godot;
using System;
using System.Collections.Generic;
using Core.Interfaces.Spawning;
using IslandSurvivor.Scenes.NPC.Agressive;

namespace IslandSurvivor.Nodes.Zones;

/// <summary>
/// [Gameplay][Spawning][Algorithme]
/// Zone responsible for spawning and managing a specific number of enemies.
/// </summary>
public partial class EnemySpawnZone : Node2D, IEnemySpawnZone
{
    [Export] public Godot.Collections.Array<EnemySpawnConfig> EnemyConfigs { get; set; } = new();

    [Export(PropertyHint.Range, "0,1,0.01")]
    public float LevelModifier { get; set; } = 0.0f;

    [Export] public Polygon2D SpawningArea { get; set; } = null!;
    [Export] public TileMapLayer WaterTileMap { get; set; } = null!;

    [Export] public float MinDistanceBetweenEnemies { get; set; } = 100f;
    [Export] public float RespawnInterval { get; set; } = 60f;

    private readonly List<EnemyBase> m_activeEnemies = new();
    private float m_respawnTimer = 0f;
    private Random m_random = new();
    private Rect2 m_cachedBounds;

    public override void _Ready()
    {
        YSortEnabled = true;

        if (SpawningArea == null)
        {
            SpawningArea = GetNodeOrNull<Polygon2D>("SpawningArea");
        }

        if (SpawningArea == null || SpawningArea.Polygon.Length < 3)
        {
            GD.PushWarning($"[EnemySpawnZone] SpawningArea is not assigned or invalid for {Name}.");
            return;
        }

        m_cachedBounds = GetPolygonBounds(SpawningArea.Polygon);
        SpawnEnemies();
    }

    public override void _Process(double p_delta)
    {
        if (SpawningArea == null || SpawningArea.Polygon.Length < 3) return;

        m_respawnTimer += (float)p_delta;
        if (m_respawnTimer >= RespawnInterval)
        {
            m_respawnTimer = 0f;
            CleanupDestroyedEnemies();
            CheckAndRespawn();
        }
    }

    /// <summary>
    /// Triggers the enemy spawning logic.
    /// </summary>
    public void SpawnEnemies()
    {
        if (EnemyConfigs == null || EnemyConfigs.Count == 0)
        {
             GD.PushWarning($"[EnemySpawnZone] No EnemyConfigs assigned for {Name}.");
             return;
        }

        CleanupDestroyedEnemies();

        foreach (var config in EnemyConfigs)
        {
            if (config == null || config.EnemyScene == null) continue;

            int currentTypeCount = GetCurrentCountForType(config.EnemyScene);
            int toSpawn = config.Count - currentTypeCount;

            for (int i = 0; i < toSpawn; i++)
            {
                TrySpawnOne(config.EnemyScene);
            }
        }

        GD.Print($"[EnemySpawnZone][Gameplay] {Name} spawned enemies. Total active: {m_activeEnemies.Count}");
    }

    private void CheckAndRespawn()
    {
        foreach (var config in EnemyConfigs)
        {
            if (config == null || config.EnemyScene == null) continue;

            int currentTypeCount = GetCurrentCountForType(config.EnemyScene);
            if (currentTypeCount < config.Count)
            {
                TrySpawnOne(config.EnemyScene);
            }
        }
    }

    private int GetCurrentCountForType(PackedScene p_scene)
    {
        int count = 0;
        foreach (var enemy in m_activeEnemies)
        {
            if (IsInstanceValid(enemy) && enemy.SceneFilePath == p_scene.ResourcePath)
            {
                count++;
            }
        }
        return count;
    }

    private bool TrySpawnOne(PackedScene p_scene)
    {
        int maxAttempts = 20;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomPoint = GenerateRandomPointInBounds();

            if (!IsInsideSpawningPolygon(randomPoint)) continue;

            Vector2 localPos = ToLocal(SpawningArea.ToGlobal(randomPoint));

            if (IsOnWater(localPos)) continue;
            if (IsTooCloseToOtherEnemies(localPos)) continue;

            SpawnEnemy(p_scene, localPos);
            return true;
        }
        return false;
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

    private bool IsOnWater(Vector2 p_localPos)
    {
        if (WaterTileMap == null) return false;

        Vector2 globalPos = ToGlobal(p_localPos);
        Vector2I tilePos = WaterTileMap.LocalToMap(WaterTileMap.ToLocal(globalPos));
        return WaterTileMap.GetCellSourceId(tilePos) != -1;
    }

    private bool IsTooCloseToOtherEnemies(Vector2 p_localPos)
    {
        foreach (var existing in m_activeEnemies)
        {
            if (IsInstanceValid(existing) && p_localPos.DistanceTo(existing.Position) < MinDistanceBetweenEnemies)
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnEnemy(PackedScene p_scene, Vector2 p_localPos)
    {
        if (p_scene == null) return;

        Node instance = p_scene.Instantiate();
        if (instance is EnemyBase enemyInstance)
        {
            // Set properties BEFORE adding to tree to ensure _Ready uses them
            enemyInstance.LevelIndex = (int)(LevelModifier * 10) + 1;

            AddChild(enemyInstance);
            enemyInstance.Position = p_localPos;
            enemyInstance.Visible = true;
            enemyInstance.YSortEnabled = true;

            // Set Collision Layer 4 (Combat) - Bit 3 (value 8)
            enemyInstance.CollisionLayer = 8;

            m_activeEnemies.Add(enemyInstance);
        }
        else
        {
            instance.QueueFree();
            GD.PushError($"[EnemySpawnZone] Instantiated scene is not an EnemyBase: {p_scene.ResourcePath}");
        }
    }

    private void CleanupDestroyedEnemies()
    {
        m_activeEnemies.RemoveAll(e => !IsInstanceValid(e));
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
