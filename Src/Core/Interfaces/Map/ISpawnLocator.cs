namespace Core.Interfaces.Map;

public interface ISpawnLocator
{
    (int x, int y) FindValidSpawn(IMapData p_mapData);
}
