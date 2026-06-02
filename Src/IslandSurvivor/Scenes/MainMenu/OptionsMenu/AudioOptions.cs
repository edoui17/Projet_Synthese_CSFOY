using Godot;
using System;
using IslandSurvivor.Globals;

public partial class AudioOptions : Control
{
    [Export] Slider masterSlider = null!;
    [Export] Slider musicSlider = null!;
    [Export] Slider sfxSlider = null!;

    private int masterBusIndex = AudioServer.GetBusIndex("Master");
    private int musicBusIndex = AudioServer.GetBusIndex("Music");
    private int sfxBusIndex = AudioServer.GetBusIndex("SFX");

    public override void _Ready()
    {
        masterSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(masterBusIndex));
        musicSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(musicBusIndex));
        sfxSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(sfxBusIndex));

        // Connect the 'drag_ended' signal to SaveSettings to avoid disk I/O on every frame
        masterSlider.DragEnded += OnSliderDragEnded;
        musicSlider.DragEnded += OnSliderDragEnded;
        sfxSlider.DragEnded += OnSliderDragEnded;
    }

    public void _on_master_soudn_h_slider_value_changed(double p_value)
    {
        float volumeDb = Mathf.LinearToDb((float)p_value);
        AudioServer.SetBusVolumeDb(masterBusIndex, volumeDb);
        AudioServer.SetBusMute(masterBusIndex, p_value <= 0.0001);
        GD.Print("Slider master Modifié");
    }

    public void _on_musicslider_value_changed(double p_value)
    {
        float volumeDb = Mathf.LinearToDb((float)p_value);
        AudioServer.SetBusVolumeDb(musicBusIndex, volumeDb);
        AudioServer.SetBusMute(musicBusIndex, p_value <= 0.0001);
        GD.Print("Slider music Modifié");
    }

    public void _on_sfx_slider_value_changed(double p_value)
    {
        float volumeDb = Mathf.LinearToDb((float)p_value);
        AudioServer.SetBusVolumeDb(sfxBusIndex, volumeDb);
        AudioServer.SetBusMute(sfxBusIndex, p_value <= 0.0001);
        GD.Print("Slider SFX Modifié");
    }

    private void OnSliderDragEnded(bool valueChanged)
    {
        if (valueChanged)
        {
            SaveSettings();
        }
    }

    private void SaveSettings()
    {
        var settingsManager = ServiceRegistry.Instance.AudioSettingsManager;
        if (settingsManager == null) return;

        var settings = settingsManager.GetSettings();
        settings.MasterVolume = (float)masterSlider.Value;
        settings.MusicVolume = (float)musicSlider.Value;
        settings.SfxVolume = (float)sfxSlider.Value;

        settingsManager.SaveSettings(settings);
    }
}
