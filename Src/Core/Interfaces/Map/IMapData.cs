namespace Core.Interfaces.Map;

public interface IMapData
{
    int Width { get; }
    int Height { get; }
    int Seed { get; }

    string GetTile(int p_x, int p_y);
    void SetTile(int p_x, int p_y, string p_tileType);
    string[,] GetGrid();
}
