# Exemple de Refactorisation de Code (Avant / Après)

## Fichier : `ResourceZone.cs` (IslandSurvivor/Nodes/Zones/)

**Avant (Code monolithique et commenté) :**
```csharp
private bool TrySpawnOne()
{
    // ... setup ...

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
```

**Après (Clean Code, Extraction de méthodes) :**
```csharp
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

private bool IsInsideSpawningPolygon(Vector2 p_point) => Geometry2D.IsPointInPolygon(p_point, SpawningArea.Polygon);

private bool IsInSafeZone(Vector2 p_localPos) => p_localPos.DistanceTo(SafeZoneCenter) < SafeZoneRadius;

private bool IsOnWater(Vector2 p_localPos)
{
    if (WaterTileMap == null) return false;
    Vector2 globalPos = ToGlobal(p_localPos);
    Vector2I tilePos = WaterTileMap.LocalToMap(WaterTileMap.ToLocal(globalPos));
    return WaterTileMap.GetCellSourceId(tilePos) != -1;
}

private bool IsTooCloseToOtherResources(Vector2 p_localPos)
{
    foreach (var existing in m_activeResources)
    {
        if (IsInstanceValid(existing) && p_localPos.DistanceTo(existing.Position) < MinDistanceBetweenResources)
        {
            return true;
        }
    }
    return false;
}
```
