namespace Core.Interfaces.Map;

public interface IMapGenerator
{
    IMapData Generate(int p_width, int p_height, int p_seed);
    void ApplyIslandConstraints(IMapData p_mapData);
}
