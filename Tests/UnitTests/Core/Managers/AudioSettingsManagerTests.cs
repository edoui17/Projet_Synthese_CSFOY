using System.Text.Json;
using Xunit;
using Core.Domain;
using Core.Interfaces;
using Core.Managers;

namespace Core.Tests.Managers;

public class AudioSettingsManagerTests
{
    private class FakeSaveService : ISaveService
    {
        private string m_savedData = string.Empty;

        public void SaveData(string p_fileName, string p_jsonData)
        {
            m_savedData = p_jsonData;
        }

        public string LoadData(string p_fileName)
        {
            return m_savedData;
        }

        public void DeleteData(string p_fileName)
        {
            m_savedData = string.Empty;
        }
    }

    [Fact]
    public void GetSettings_WhenNoDataExists_ReturnsDefaultSettings()
    {
        // Arrange
        var fakeSaveService = new FakeSaveService();
        var manager = new AudioSettingsManager(fakeSaveService);

        // Act
        var settings = manager.GetSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(1.0f, settings.MasterVolume);
        Assert.Equal(1.0f, settings.MusicVolume);
        Assert.Equal(1.0f, settings.SfxVolume);
    }

    [Fact]
    public void SaveAndGetSettings_PersistsAndLoadsCorrectData()
    {
        // Arrange
        var fakeSaveService = new FakeSaveService();
        var manager = new AudioSettingsManager(fakeSaveService);
        var expectedSettings = new AudioSettingsData
        {
            MasterVolume = 0.5f,
            MusicVolume = 0.2f,
            SfxVolume = 0.8f
        };

        // Act
        manager.SaveSettings(expectedSettings);
        var loadedSettings = manager.GetSettings();

        // Assert
        Assert.NotNull(loadedSettings);
        Assert.Equal(0.5f, loadedSettings.MasterVolume);
        Assert.Equal(0.2f, loadedSettings.MusicVolume);
        Assert.Equal(0.8f, loadedSettings.SfxVolume);
    }
}
