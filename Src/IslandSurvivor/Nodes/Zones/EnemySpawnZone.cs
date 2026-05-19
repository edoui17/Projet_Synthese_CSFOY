using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces.Spawning;
using Core.Interfaces.Utils;
using Core.Utils;
using IslandSurvivor.Scenes.NPC.Aggressive;
using IslandSurvivor.Utils;
using IslandSurvivor.Utils.Logging;

namespace IslandSurvivor.Nodes.Zones;

/// <summary>
/// [Gameplay][Spawning][Algorithme]
/// Zone responsible for spawning and managing a specific number of enemies using weighted probabilities.
/// </summary>
public partial class EnemySpawnZone : Node2D, IEnemySpawnZone
{
    [Export] public Godot.Collections.Array<EnemySpawnConfig> EnemyConfigs { get; set; } = new();

    [Export] public int MaxEnemies { get; set; } = 5;

    [Export] public Polygon2D SpawningArea { get; set; } = null!;
    [Export] public TileMapLayer WaterTileMap { get; set; } = null!;

    [Export] public float MinDistanceBetweenEnemies { get; set; } = 100f;
    [Export] public float RespawnInterval { get; set; } = 60f;

    private readonly List<AggressiveNpcBase> m_activeEnemies = new();
    private float m_respawnTimer = 0f;
    private Rect2 m_cachedBounds;
    private readonly IWeightedRandomSelector<EnemySpawnConfig> m_weightedSelector = new WeightedRandomSelector<EnemySpawnConfig>(new GodotRandomProvider(), new GodotLogger());

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

        int toSpawn = MaxEnemies - m_activeEnemies.Count;

        for (int i = 0; i < toSpawn; i++)
        {
            TrySpawnWeightedEnemy();
        }

        GD.Print($"[EnemySpawnZone][Gameplay] {Name} spawned enemies. Total active: {m_activeEnemies.Count}");
    }

    private void CheckAndRespawn()
    {
        int toSpawn = MaxEnemies - m_activeEnemies.Count;
        for (int i = 0; i < toSpawn; i++)
        {
            if (!TrySpawnWeightedEnemy()) break;
        }
    }

    private bool TrySpawnWeightedEnemy()
    {
        var config = m_weightedSelector.SelectRandom(EnemyConfigs);
        if (config == null || config.EnemyScene == null) return false;

        int maxAttempts = 20;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomPoint = GenerateRandomPointInBounds();

            if (!IsInsideSpawningPolygon(randomPoint)) continue;

            Vector2 localPos = ToLocal(SpawningArea.ToGlobal(randomPoint));

            if (IsOnWater(localPos)) continue;
            if (IsTooCloseToOtherEnemies(localPos)) continue;

            SpawnEnemy(config.EnemyScene, localPos);
            return true;
        }
        return false;
    }

    private Vector2 GenerateRandomPointInBounds()
    {
        return new Vector2(
            GD.Randf() * m_cachedBounds.Size.X + m_cachedBounds.Position.X,
            GD.Randf() * m_cachedBounds.Size.Y + m_cachedBounds.Position.Y
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
        if (instance is AggressiveNpcBase enemyInstance)
        {
            AddChild(enemyInstance);
            enemyInstance.Position = p_localPos;
            enemyInstance.Visible = true;
            enemyInstance.YSortEnabled = true;


            m_activeEnemies.Add(enemyInstance);
        }
        else
        {
            instance.QueueFree();
            GD.PushError($"[EnemySpawnZone] Instantiated scene is not an AggressiveNpcBase: {p_scene.ResourcePath}");
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
