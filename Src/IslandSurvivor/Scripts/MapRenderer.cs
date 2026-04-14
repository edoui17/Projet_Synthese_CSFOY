using Godot;
using Core.Interfaces.Map;
using Core.Models.Map;
using IslandSurvivor.Globals;
using System.Collections.Generic;

#nullable enable

namespace IslandSurvivor.Scripts;

public partial class MapRenderer : Node2D
{
    [Export] public TileMapLayer? WaterTileMap { get; set; }
    [Export] public TileMapLayer? FoamWaterTileMap { get; set; }
    [Export] public TileMapLayer? GroundTileMap { get; set; }
    [Export] public TileMapLayer? ShadowTileMap { get; set; }
    [Export] public TileMapLayer? ElevationTileMap { get; set; }
    [Export] public TileMapLayer? ElementsTileMap { get; set; }
    [Export] public TileMapLayer? ElementsForElevationTileMap { get; set; }
    [Export] public TileMapLayer? CloudTileMap { get; set; }
    [Export] public Marker2D? SpawnMarker { get; set; }

    [Export] public int MapWidth { get; set; } = 100;
    [Export] public int MapHeight { get; set; } = 100;
    [Export] public int MapSeed { get; set; } = 12345;

    public override void _Ready()
    {
        base._Ready();

        if (GameManager.Instance == null || GameManager.Instance.MapManager == null)
        {
            GD.PrintErr("MapRenderer: GameManager or MapManager is not available.");
            return;
        }

        // Generate map via Manager
        IMapData mapData = GameManager.Instance.MapManager.GenerateNewMap(MapWidth, MapHeight, MapSeed);

        // Render the generated map
        RenderMap(mapData);

        // Update spawn marker
        if (SpawnMarker != null)
        {
            (int sx, int sy) = GameManager.Instance.MapManager.GetPlayerSpawnPoint();
            // Assuming tile size is 64x64, we position the marker at the center of the tile
            SpawnMarker.Position = new Vector2(sx * 64 + 32, sy * 64 + 32);
        }
    }

    private void RenderMap(IMapData p_mapData)
    {
        ClearAllTileMaps();

        List<Vector2I> groundCells = new List<Vector2I>();
        List<Vector2I> elevationCells = new List<Vector2I>();
        List<Vector2I> cliffCells = new List<Vector2I>();

        // Pass 1: Collect coordinates and set water
        for (int x = 0; x < p_mapData.Width; x++)
        {
            for (int y = 0; y < p_mapData.Height; y++)
            {
                string tileType = p_mapData.GetTile(x, y);
                Vector2I pos = new Vector2I(x, y);

                if (tileType == TileTypeConstants.WATER)
                {
                    // Assuming water is source 0, atlas coords (0,0) based on typical tilemaps
                    // Map details showed water tile map with atlas source 0:0/0
                    WaterTileMap?.SetCell(pos, 0, new Vector2I(0, 0));
                }
                else if (tileType == TileTypeConstants.GROUND)
                {
                    groundCells.Add(pos);
                }
                else if (tileType == TileTypeConstants.PLATEAU)
                {
                    elevationCells.Add(pos);
                }
                else if (tileType == TileTypeConstants.CLIFF || tileType == TileTypeConstants.STAIRS)
                {
                    cliffCells.Add(pos);
                }
            }
        }

        // Pass 2: TerrainConnect for Ground
        if (groundCells.Count > 0 && GroundTileMap != null)
        {
            // Assuming terrain set 0, terrain 0 is Ground
            Godot.Collections.Array<Vector2I> arr = new Godot.Collections.Array<Vector2I>(groundCells);
            GroundTileMap.SetCellsTerrainConnect(arr, 0, 0);
        }

        // Pass 3: TerrainConnect for Elevation
        if (ElevationTileMap != null)
        {
            if (elevationCells.Count > 0)
            {
                // Plateau (terrain 0)
                Godot.Collections.Array<Vector2I> elevArr = new Godot.Collections.Array<Vector2I>(elevationCells);
                ElevationTileMap.SetCellsTerrainConnect(elevArr, 0, 0);
            }

            if (cliffCells.Count > 0)
            {
                // Cliff/Stairs (terrain 1 usually, assuming cliff is terrain 1)
                Godot.Collections.Array<Vector2I> cliffArr = new Godot.Collections.Array<Vector2I>(cliffCells);
                ElevationTileMap.SetCellsTerrainConnect(cliffArr, 0, 1);
            }
        }

        // Pass 4: Foam calculation (Adjacency)
        if (FoamWaterTileMap != null)
        {
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int x = 0; x < p_mapData.Width; x++)
            {
                for (int y = 0; y < p_mapData.Height; y++)
                {
                    string tileType = p_mapData.GetTile(x, y);
                    if (tileType == TileTypeConstants.GROUND || tileType == TileTypeConstants.PLATEAU || tileType == TileTypeConstants.CLIFF || tileType == TileTypeConstants.STAIRS)
                    {
                        bool hasWaterAdjacent = false;
                        for (int i = 0; i < 4; i++)
                        {
                            int nx = x + dx[i];
                            int ny = y + dy[i];

                            // Out of bounds is treated as water effectively
                            if (nx < 0 || nx >= p_mapData.Width || ny < 0 || ny >= p_mapData.Height)
                            {
                                hasWaterAdjacent = true;
                                break;
                            }
                            else if (p_mapData.GetTile(nx, ny) == TileTypeConstants.WATER)
                            {
                                hasWaterAdjacent = true;
                                break;
                            }
                        }

                        if (hasWaterAdjacent)
                        {
                            // Place foam under the ground tile
                            // Note: FoamWaterTileMap has source 0 for animated foam. Atlas coord (0,0).
                            FoamWaterTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(0, 0));
                        }
                    }
                }
            }
        }
    }

    private void ClearAllTileMaps()
    {
        WaterTileMap?.Clear();
        FoamWaterTileMap?.Clear();
        GroundTileMap?.Clear();
        ShadowTileMap?.Clear();
        ElevationTileMap?.Clear();
        ElementsTileMap?.Clear();
        ElementsForElevationTileMap?.Clear();
        // CloudTileMap?.Clear(); // Usually clouds are visual overlay, don't clear unless requested
    }
}
