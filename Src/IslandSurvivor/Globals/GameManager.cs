using Godot;
using Core.Managers.Map;
using IslandSurvivor.Generators;
using Core.Interfaces.Map;

namespace IslandSurvivor.Globals;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public MapManager MapManager { get; private set; }

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;

        IMapGenerator mapGenerator = new GodotIslandGenerator();
        ISpawnLocator spawnLocator = new BasicSpawnLocator();

        MapManager = new MapManager(mapGenerator, spawnLocator);
    }
}
