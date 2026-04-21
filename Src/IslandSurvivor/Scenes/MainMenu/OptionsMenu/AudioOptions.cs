using Godot;
using System;

public partial class AudioOptions : Control
{
    [Export] HSlider masterSlider;
    [Export] HSlider musicSlider;
    [Export] HSlider sfxSlider;

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
        AudioServer.SetBusVolumeDb(masterBusIndex, Mathf.LinearToDb((float)p_value));
        GD.Print("Slider master Modifié");
    }

    public void _on_musicslider_value_changed(double p_value)
    {
        AudioServer.SetBusVolumeDb(musicBusIndex, Mathf.LinearToDb((float)p_value));
        GD.Print("Slider music Modifié");
    }

    public void _on_sfx_slider_value_changed(double p_value)
    {
        AudioServer.SetBusVolumeDb(sfxBusIndex, Mathf.LinearToDb((float)p_value));
        GD.Print("Slider SFX Modifié");
    }
}
