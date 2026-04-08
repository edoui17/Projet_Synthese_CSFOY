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

                // Base threshold for ground vs water, and plateau threshold
                if (noiseValue > 0.4f)
                {
                    mapData.SetTile(x, y, TileTypeConstants.PLATEAU);
                }
                else if (noiseValue > 0.0f)
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

        // 2. Generate Cliffs and Stairs for Plateaus
        GenerateCliffsAndStairs(p_mapData);

        // 3. Ensure connected landmass (Flood Fill / BFS)
        // Find a starting point near the center that is GROUND or STAIRS
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
                    if (!visited[nx, ny])
                    {
                        string tile = p_mapData.GetTile(nx, ny);
                        if (tile == TileTypeConstants.GROUND || tile == TileTypeConstants.STAIRS || tile == TileTypeConstants.PLATEAU)
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue((nx, ny));
                        }
                    }
                }
            }
        }

        // 4. Remove unconnected ground/plateau/stairs/cliff tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                string tile = p_mapData.GetTile(x, y);
                if (tile != TileTypeConstants.WATER && !visited[x, y])
                {
                    // A cliff adjacent to a visited plateau/stairs/ground is valid, otherwise it's removed
                    bool adjacentToVisited = false;
                    if (tile == TileTypeConstants.CLIFF)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            int nx = x + dx[i];
                            int ny = y + dy[i];
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height && visited[nx, ny])
                            {
                                adjacentToVisited = true;
                                break;
                            }
                        }
                    }

                    if (!adjacentToVisited)
                    {
                        p_mapData.SetTile(x, y, TileTypeConstants.WATER);
                    }
                }
            }
        }
    }

    private void GenerateCliffsAndStairs(IMapData p_mapData)
    {
        int width = p_mapData.Width;
        int height = p_mapData.Height;
        bool[,] plateauVisited = new bool[width, height];

        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (p_mapData.GetTile(x, y) == TileTypeConstants.PLATEAU && !plateauVisited[x, y])
                {
                    // Start BFS to find the whole plateau and its borders
                    List<(int px, int py)> plateauTiles = new List<(int, int)>();
                    List<(int cx, int cy)> borderTiles = new List<(int, int)>();

                    Queue<(int qx, int qy)> queue = new Queue<(int, int)>();
                    queue.Enqueue((x, y));
                    plateauVisited[x, y] = true;

                    while (queue.Count > 0)
                    {
                        var curr = queue.Dequeue();
                        plateauTiles.Add(curr);

                        bool isBorder = false;

                        for (int i = 0; i < 4; i++)
                        {
                            int nx = curr.qx + dx[i];
                            int ny = curr.qy + dy[i];

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                string neighborTile = p_mapData.GetTile(nx, ny);
                                if (neighborTile == TileTypeConstants.PLATEAU)
                                {
                                    if (!plateauVisited[nx, ny])
                                    {
                                        plateauVisited[nx, ny] = true;
                                        queue.Enqueue((nx, ny));
                                    }
                                }
                                else if (neighborTile == TileTypeConstants.GROUND)
                                {
                                    isBorder = true;
                                }
                            }
                        }

                        if (isBorder)
                        {
                            borderTiles.Add(curr);
                        }
                    }

                    // Turn all borders of this plateau into CLIFF
                    foreach (var border in borderTiles)
                    {
                        p_mapData.SetTile(border.cx, border.cy, TileTypeConstants.CLIFF);
                    }

                    // Ensure at least one STAIRS access
                    if (borderTiles.Count > 0)
                    {
                        // To make it simple, pick the first border tile that is adjacent to GROUND and turn it to STAIRS
                        foreach (var border in borderTiles)
                        {
                            bool hasGroundAdjacent = false;
                            for (int i = 0; i < 4; i++)
                            {
                                int nx = border.cx + dx[i];
                                int ny = border.cy + dy[i];
                                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                {
                                    if (p_mapData.GetTile(nx, ny) == TileTypeConstants.GROUND)
                                    {
                                        hasGroundAdjacent = true;
                                        break;
                                    }
                                }
                            }

                            if (hasGroundAdjacent)
                            {
                                p_mapData.SetTile(border.cx, border.cy, TileTypeConstants.STAIRS);
                                break; // Only need one stair per plateau (can be more, but one guarantees access)
                            }
                        }
                    }
                    else
                    {
                        // Completely isolated plateau (no ground adjacent). Turn entirely to water or ground.
                        // Here we just turn it back to ground.
                        foreach (var pt in plateauTiles)
                        {
                            p_mapData.SetTile(pt.px, pt.py, TileTypeConstants.GROUND);
                        }
                    }
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
