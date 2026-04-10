using System;
using Core.Interfaces.Map;
using Core.Models.Map;

namespace Core.Utils.Map;

public class SpawnLocator : ISpawnLocator
{
    public (int x, int y) FindValidSpawn(IMapData p_mapData)
    {
        int centerX = p_mapData.Width / 2;
        int centerY = p_mapData.Height / 2;

        int maxRadius = Math.Min(centerX, centerY);

        // Search outward from the center
        for (int r = 0; r < maxRadius; r++)
        {
            for (int x = centerX - r; x <= centerX + r; x++)
            {
                for (int y = centerY - r; y <= centerY + r; y++)
                {
                    // Check perimeter of the current search radius
                    if (x == centerX - r || x == centerX + r || y == centerY - r || y == centerY + r)
                    {
                        if (x > 0 && x < p_mapData.Width - 1 && y > 0 && y < p_mapData.Height - 1)
                        {
                            if (IsValidSpawnTile(p_mapData, x, y))
                            {
                                return (x, y);
                            }
                        }
                    }
                }
            }
        }

        // Failsafe: if no totally clear tile is found, just find the first ground tile.
        for (int x = 0; x < p_mapData.Width; x++)
        {
            for (int y = 0; y < p_mapData.Height; y++)
            {
                if (p_mapData.GetTile(x, y) == TileTypeConstants.GROUND)
                {
                    return (x, y);
                }
            }
        }

        return (centerX, centerY); // Absolute fallback
    }

    private bool IsValidSpawnTile(IMapData p_mapData, int p_x, int p_y)
    {
        if (p_mapData.GetTile(p_x, p_y) != TileTypeConstants.GROUND)
        {
            return false;
        }

        // Check surrounding 8 tiles to ensure the player isn't boxed in or touching water immediately
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        int groundCount = 0;

        for (int i = 0; i < 8; i++)
        {
            int nx = p_x + dx[i];
            int ny = p_y + dy[i];

            if (nx >= 0 && nx < p_mapData.Width && ny >= 0 && ny < p_mapData.Height)
            {
                if (p_mapData.GetTile(nx, ny) == TileTypeConstants.GROUND)
                {
                    groundCount++;
                }
            }
        }

        // Require all surrounding tiles to be ground for a perfectly safe spawn
        return groundCount == 8;
    }
}
