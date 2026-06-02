using Core.Domain;

namespace Core.Interfaces;

public interface IAudioSettingsManager
{
    AudioSettingsData GetSettings();
    void SaveSettings(AudioSettingsData p_settings);
}
