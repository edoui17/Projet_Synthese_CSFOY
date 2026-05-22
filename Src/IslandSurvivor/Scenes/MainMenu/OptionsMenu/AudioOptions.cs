using Godot;
using System;

public partial class AudioOptions : Control
{
    [Export] HSlider masterSlider = null!;
    [Export] HSlider musicSlider = null!;
    [Export] HSlider sfxSlider = null!;

    private int masterBusIndex = AudioServer.GetBusIndex("Master");
    private int musicBusIndex = AudioServer.GetBusIndex("Music");
    private int sfxBusIndex = AudioServer.GetBusIndex("SFX");

    public override void _Ready()
    {
        masterSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(masterBusIndex));
        musicSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(musicBusIndex));
        sfxSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(sfxBusIndex));
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
}
