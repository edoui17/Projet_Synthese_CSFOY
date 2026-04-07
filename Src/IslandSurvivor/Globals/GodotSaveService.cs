using Godot;
using Core.Interfaces;

namespace IslandSurvivor.Globals;

public class GodotSaveService : ISaveService
{
    private const string SAVE_DIR = "user://";

    public void SaveData(string p_fileName, string p_jsonData)
    {
        string path = SAVE_DIR + p_fileName;
        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        if (file == null)
        {
            GD.PrintErr($"GodotSaveService: Error opening file '{path}' for writing. Error: {FileAccess.GetOpenError()}");
            return;
        }

        file.StoreString(p_jsonData);
        file.Close();
    }

    public string LoadData(string p_fileName)
    {
        string path = SAVE_DIR + p_fileName;

        if (!FileAccess.FileExists(path))
        {
            return string.Empty;
        }

        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null)
        {
            GD.PrintErr($"GodotSaveService: Error opening file '{path}' for reading. Error: {FileAccess.GetOpenError()}");
            return string.Empty;
        }

        string content = file.GetAsText();
        file.Close();

        return content;
    }
}
