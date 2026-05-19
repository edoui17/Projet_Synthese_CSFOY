using Godot;
using Core.Interfaces;

namespace IslandSurvivor.Globals;

public class GodotSaveService : ISaveService
{
    private const string OLD_SAVE_DIR = "user://";
    private readonly string m_saveDir;

    public GodotSaveService()
    {
        // res:// is Src/IslandSurvivor/, so res://../../ is the .sln root.
        string rootPath = ProjectSettings.GlobalizePath("res://../../");
        m_saveDir = rootPath + "Save/";

        EnsureDirectoryExists();
        MigrateOldSaves();
    }

    private void EnsureDirectoryExists()
    {
        if (!DirAccess.DirExistsAbsolute(m_saveDir))
        {
            Error err = DirAccess.MakeDirAbsolute(m_saveDir);
            if (err != Error.Ok)
            {
                GD.PrintErr($"GodotSaveService: Error creating Save directory '{m_saveDir}'. Error: {err}");
            }
        }
    }

    private void MigrateOldSaves()
    {
        using var oldDir = DirAccess.Open(OLD_SAVE_DIR);
        if (oldDir != null)
        {
            oldDir.ListDirBegin();
            string fileName = oldDir.GetNext();
            while (fileName != "")
            {
                if (!oldDir.CurrentIsDir() && fileName.EndsWith(".json"))
                {
                    string oldPath = OLD_SAVE_DIR + fileName;
                    string newPath = m_saveDir + fileName;

                    // Only migrate if it doesn't already exist in the new directory
                    if (!FileAccess.FileExists(newPath))
                    {
                        Error err = oldDir.Copy(oldPath, newPath);
                        if (err == Error.Ok)
                        {
                            GD.Print($"GodotSaveService: Successfully migrated {fileName} to new Save directory.");
                        }
                        else
                        {
                            GD.PrintErr($"GodotSaveService: Failed to migrate {fileName}. Error: {err}");
                        }
                    }
                }
                fileName = oldDir.GetNext();
            }
        }
    }

    public void SaveData(string p_fileName, string p_jsonData)
    {
        EnsureDirectoryExists();
        // SECURITY FIX: Sanitize input to prevent Path Traversal
        string safeFileName = System.IO.Path.GetFileName(p_fileName);
        string path = System.IO.Path.Combine(m_saveDir, safeFileName);
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
        // SECURITY FIX: Sanitize input to prevent Path Traversal
        string safeFileName = System.IO.Path.GetFileName(p_fileName);
        string path = System.IO.Path.Combine(m_saveDir, safeFileName);

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
