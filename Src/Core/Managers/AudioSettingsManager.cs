using System.Text.Json;
using Core.Domain;
using Core.Interfaces;

namespace Core.Managers;

public class AudioSettingsManager : IAudioSettingsManager
{
    private readonly ISaveService m_saveService;
    private const string FILE_NAME = "audio_settings.json";

    public AudioSettingsManager(ISaveService p_saveService)
    {
        m_saveService = p_saveService;
    }

    public AudioSettingsData GetSettings()
    {
        string jsonData = m_saveService.LoadData(FILE_NAME);
        if (string.IsNullOrEmpty(jsonData))
        {
            return new AudioSettingsData(); // Returns defaults (1.0f)
        }

        try
        {
            var settings = JsonSerializer.Deserialize<AudioSettingsData>(jsonData);
            return settings ?? new AudioSettingsData();
        }
        catch
        {
            return new AudioSettingsData();
        }
    }

    public void SaveSettings(AudioSettingsData p_settings)
    {
        string jsonData = JsonSerializer.Serialize(p_settings);
        m_saveService.SaveData(FILE_NAME, jsonData);
    }
}
