using Godot;
using Core.Managers.Map;
using Core.Utils.Map;
using IslandSurvivor.Generators;
using Core.Models.Map;
using Core.Interfaces.Map;

namespace IslandSurvivor.Scenes.TestScenes;

public partial class ProceduralMapTest : Node2D
{
    private MapManager m_mapManager;

    private Button m_generateButton;
    private LineEdit m_seedLineEdit;
    private TileMapLayer m_groundTileMap;
    private TileMapLayer m_waterTileMap;
    private ColorRect m_playerMarker;

    public override void _Ready()
    {
        // Initialize Core System
        GodotIslandGenerator generator = new GodotIslandGenerator();
        SpawnLocator locator = new SpawnLocator();
        m_mapManager = new MapManager(generator, locator);

        // UI Setup
        m_generateButton = GetNode<Button>("UI/Panel/VBoxContainer/GenerateButton");
        m_seedLineEdit = GetNode<LineEdit>("UI/Panel/VBoxContainer/SeedLineEdit");
        m_playerMarker = GetNode<ColorRect>("PlayerMarker");

        m_generateButton.Pressed += OnGenerateButtonPressed;

        // The map_1.tscn instances a few TileMapLayers
        m_groundTileMap = GetNode<TileMapLayer>("MapContainer/Map1/GroundTileMap");
        m_waterTileMap = GetNode<TileMapLayer>("MapContainer/Map1/WaterTileMap");

        // Generate initial map
        GenerateAndDrawMap(12345);
    }

    private void OnGenerateButtonPressed()
    {
        int seed = 12345;
        if (!string.IsNullOrEmpty(m_seedLineEdit.Text))
        {
            if (int.TryParse(m_seedLineEdit.Text, out int parsedSeed))
            {
                seed = parsedSeed;
            }
        }
        GenerateAndDrawMap(seed);
    }

    private void GenerateAndDrawMap(int p_seed)
    {
        int mapWidth = 64;
        int mapHeight = 64;

        // Generate logic in Core
        IMapData mapData = m_mapManager.GenerateNewMap(mapWidth, mapHeight, p_seed);

        // Clear existing maps
        m_groundTileMap.Clear();
        m_waterTileMap.Clear();

        // Tile constants based on map_1.tscn
        // Water is TileSet 0 in WaterTileMap. Coordinates: (0,0) (Atlas source 0)
        // Ground is TileSet 0 in GroundTileMap. Coordinates: (0,3) for standard ground (Atlas source 0)

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector2I cellPos = new Vector2I(x, y);
                string tileType = mapData.GetTile(x, y);

                if (tileType == TileTypeConstants.GROUND)
                {
                    // Draw ground
                    m_groundTileMap.SetCell(cellPos, 0, new Vector2I(0, 3));
                }
                else if (tileType == TileTypeConstants.WATER)
                {
                    // Draw water
                    m_waterTileMap.SetCell(cellPos, 0, new Vector2I(0, 0));
                }
            }
        }

        // Setup spawn point
        var spawnPoint = m_mapManager.GetPlayerSpawnPoint();

        // Convert to world coordinates. Tile size in map_1 is 64x64
        Vector2 worldPos = m_groundTileMap.MapToLocal(new Vector2I(spawnPoint.x, spawnPoint.y));

        // Adjust for Map1 local offset if any, but since they are child of MapContainer which is at 0,0,
        // worldPos of TileMapLayer matches local node space.
        m_playerMarker.GlobalPosition = m_groundTileMap.ToGlobal(worldPos) - new Vector2(20, 20); // Center the 40x40 marker

        // Center camera roughly on the island
        GetNode<Camera2D>("Camera2D").GlobalPosition = m_groundTileMap.ToGlobal(m_groundTileMap.MapToLocal(new Vector2I(mapWidth / 2, mapHeight / 2)));
    }
}
