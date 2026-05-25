using System;
using System.Security.Cryptography;
using System.Text;
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

        // SECURITY ENHANCEMENT: Implement SHA256 integrity checksum verification to flag file tampering.
        string checksumPath = path + ".sig";
        using FileAccess sigFile = FileAccess.Open(checksumPath, FileAccess.ModeFlags.Write);
        if (sigFile != null)
        {
            sigFile.StoreString(ComputeChecksum(p_jsonData));
            sigFile.Close();
        }
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

        // SECURITY ENHANCEMENT: Verify SHA256 integrity checksum to prevent data tampering.
        string checksumPath = path + ".sig";
        if (FileAccess.FileExists(checksumPath))
        {
            using FileAccess sigFile = FileAccess.Open(checksumPath, FileAccess.ModeFlags.Read);
            if (sigFile != null)
            {
                string expectedChecksum = sigFile.GetAsText().Trim();
                sigFile.Close();

                if (expectedChecksum != ComputeChecksum(content))
                {
                    GD.PrintErr($"[SECURITY LOG] GodotSaveService: Checksum validation failed for '{path}'. Data may have been tampered with.");
                    return string.Empty;
                }
            }
        }
        else
        {
            // If signature doesn't exist, we fallback to returning the content (e.g., old saves)
            // Or log a warning.
            GD.Print($"GodotSaveService: No signature found for '{path}'. Proceeding without validation.");
        }

        return content;
    }

    public void DeleteData(string p_fileName)
    {
        string safeFileName = System.IO.Path.GetFileName(p_fileName);
        string path = System.IO.Path.Combine(m_saveDir, safeFileName);
        string checksumPath = path + ".sig";

        if (FileAccess.FileExists(path))
        {
            DirAccess.RemoveAbsolute(path);
            GD.Print($"GodotSaveService: Deleted '{path}'.");
        }

        if (FileAccess.FileExists(checksumPath))
        {
            DirAccess.RemoveAbsolute(checksumPath);
            GD.Print($"GodotSaveService: Deleted checksum '{checksumPath}'.");
        }
    }

    private static string ComputeChecksum(string p_data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(p_data));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
