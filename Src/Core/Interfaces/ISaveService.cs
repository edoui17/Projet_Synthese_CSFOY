namespace Core.Interfaces;

public interface ISaveService
{
    void SaveData(string p_fileName, string p_jsonData);
    string LoadData(string p_fileName);
}
