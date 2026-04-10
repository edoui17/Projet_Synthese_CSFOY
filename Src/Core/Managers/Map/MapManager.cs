using Core.Interfaces.Map;

namespace Core.Managers.Map;

public class MapManager
{
    private readonly IMapGenerator m_mapGenerator;
    private readonly ISpawnLocator m_spawnLocator;

    private IMapData? m_currentMap;
    public IMapData? CurrentMap => m_currentMap;

    public MapManager(IMapGenerator p_mapGenerator, ISpawnLocator p_spawnLocator)
    {
        m_mapGenerator = p_mapGenerator;
        m_spawnLocator = p_spawnLocator;
    }

    public IMapData GenerateNewMap(int p_width, int p_height, int p_seed)
    {
        m_currentMap = m_mapGenerator.Generate(p_width, p_height, p_seed);
        m_mapGenerator.ApplyIslandConstraints(m_currentMap);
        return m_currentMap;
    }

    public (int x, int y) GetPlayerSpawnPoint()
    {
        if (m_currentMap == null)
        {
            throw new System.InvalidOperationException("Map has not been generated yet.");
        }

        return m_spawnLocator.FindValidSpawn(m_currentMap);
    }
}
