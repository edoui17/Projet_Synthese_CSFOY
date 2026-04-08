using Godot;
using Core.Interfaces.Map;
using Core.Models.Map;
using System.Collections.Generic;

namespace IslandSurvivor.Generators;

public class GodotIslandGenerator : IMapGenerator
{
    private FastNoiseLite m_noise;

    public GodotIslandGenerator()
    {
        m_noise = new FastNoiseLite();
        m_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        m_noise.Seed = 0;
        m_noise.Frequency = 0.05f;
    }

    public IMapData Generate(int p_width, int p_height, int p_seed)
    {
        IMapData mapData = new MapData(p_width, p_height, p_seed);
        m_noise.Seed = p_seed;

        for (int x = 0; x < p_width; x++)
        {
            for (int y = 0; y < p_height; y++)
            {
                // GetNoise2D returns values from -1 to 1
                float noiseValue = m_noise.GetNoise2D(x, y);

                // Base threshold for ground vs water
                if (noiseValue > 0.0f)
                {
                    mapData.SetTile(x, y, TileTypeConstants.GROUND);
                }
                else
                {
                    mapData.SetTile(x, y, TileTypeConstants.WATER);
                }
            }
        }

        return mapData;
    }

    public void ApplyIslandConstraints(IMapData p_mapData)
    {
        int width = p_mapData.Width;
        int height = p_mapData.Height;
        int centerX = width / 2;
        int centerY = height / 2;

        // 1. Force edges to be water and apply a circular mask to make it an island
        float maxDistance = Mathf.Min(centerX, centerY);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distanceToCenter = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

                // If it's near the edge or outside the circle, force it to water
                if (distanceToCenter > maxDistance * 0.9f)
                {
                    p_mapData.SetTile(x, y, TileTypeConstants.WATER);
                }
                else if (distanceToCenter > maxDistance * 0.6f)
                {
                    // Gradient mask for natural transition
                    float gradient = 1.0f - ((distanceToCenter - (maxDistance * 0.6f)) / (maxDistance * 0.3f));
                    // Multiply existing noise to lower it near the edges
                    float noiseValue = m_noise.GetNoise2D(x, y);
                    float maskedValue = (noiseValue + 1.0f) * 0.5f * gradient; // Normalize to 0-1 and apply gradient

                    if (maskedValue < 0.45f)
                    {
                        p_mapData.SetTile(x, y, TileTypeConstants.WATER);
                    }
                }
            }
        }

        // 2. Ensure connected landmass (Flood Fill / BFS)
        // Find a starting point near the center that is GROUND
        (int startX, int startY) = FindCentralGroundTile(p_mapData, centerX, centerY);

        if (startX == -1)
        {
            // Failsafe: force a center tile to be ground if no ground exists
            p_mapData.SetTile(centerX, centerY, TileTypeConstants.GROUND);
            startX = centerX;
            startY = centerY;
        }

        bool[,] visited = new bool[width, height];
        Queue<(int x, int y)> queue = new Queue<(int x, int y)>();

        queue.Enqueue((startX, startY));
        visited[startX, startY] = true;

        // Valid directions for 4-way connectivity
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    if (!visited[nx, ny] && p_mapData.GetTile(nx, ny) == TileTypeConstants.GROUND)
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }

        // 3. Remove unconnected ground tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (p_mapData.GetTile(x, y) == TileTypeConstants.GROUND && !visited[x, y])
                {
                    p_mapData.SetTile(x, y, TileTypeConstants.WATER);
                }
            }
        }
    }

    private (int x, int y) FindCentralGroundTile(IMapData p_mapData, int p_centerX, int p_centerY)
    {
        // Simple outward spiral search or box search to find closest ground tile
        int maxRadius = Mathf.Min(p_centerX, p_centerY);

        for (int r = 0; r < maxRadius; r++)
        {
            for (int x = p_centerX - r; x <= p_centerX + r; x++)
            {
                for (int y = p_centerY - r; y <= p_centerY + r; y++)
                {
                    // Check if on the perimeter of the current box
                    if (x == p_centerX - r || x == p_centerX + r || y == p_centerY - r || y == p_centerY + r)
                    {
                        if (x >= 0 && x < p_mapData.Width && y >= 0 && y < p_mapData.Height)
                        {
                            if (p_mapData.GetTile(x, y) == TileTypeConstants.GROUND)
                            {
                                return (x, y);
                            }
                        }
                    }
                }
            }
        }

        return (-1, -1);
    }
}
