using Core.Interfaces.Map;

namespace Core.Models.Map;

public class MapData : IMapData
{
    public int Width { get; }
    public int Height { get; }
    public int Seed { get; }

    private readonly string[,] m_grid;

    public MapData(int p_width, int p_height, int p_seed)
    {
        Width = p_width;
        Height = p_height;
        Seed = p_seed;
        m_grid = new string[p_width, p_height];
    }

    public string GetTile(int p_x, int p_y)
    {
        if (p_x < 0 || p_x >= Width || p_y < 0 || p_y >= Height)
        {
            return TileTypeConstants.WATER;
        }
        return m_grid[p_x, p_y];
    }

    public void SetTile(int p_x, int p_y, string p_tileType)
    {
        if (p_x >= 0 && p_x < Width && p_y >= 0 && p_y < Height)
        {
            m_grid[p_x, p_y] = p_tileType;
        }
    }

    public string[,] GetGrid()
    {
        return m_grid;
    }
}
