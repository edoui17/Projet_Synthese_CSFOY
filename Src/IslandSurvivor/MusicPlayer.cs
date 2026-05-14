using Godot;
using System;

public partial class MusicPlayer : AudioStreamPlayer
{
    public override void _Ready()
    {
        Play();
    }

    public void PlayMusic(AudioStream p_music)
    {
        if (Stream == p_music) return;

        Stream = p_music;
        Play();
    }
}
