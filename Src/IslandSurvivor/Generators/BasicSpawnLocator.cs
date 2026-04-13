using Core.Interfaces.Map;
using Godot;

namespace IslandSurvivor.Generators;

public class BasicSpawnLocator : ISpawnLocator
{
    public (int x, int y) FindValidSpawn(IMapData p_mapData)
    {
        // Simple fallback locator. Typically, we'd search for a valid ground tile.
        // For now, let's just find the first Ground tile.
        for (int y = 0; y < p_mapData.Height; y++)
        {
            for (int x = 0; x < p_mapData.Width; x++)
            {
                if (p_mapData.GetTile(x, y) == Core.Models.Map.TileTypeConstants.GROUND)
                {
                    return (x, y);
                }
            }
        }

        return (p_mapData.Width / 2, p_mapData.Height / 2);
    }
}
