using Godot;
using System;

public partial class MusicPlayer : AudioStreamPlayer
{
    private AudioStreamPlayer _player;

    public override void _Ready()
    {
        _player = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _player.Play();
    }

    public void PlayMusic(AudioStream p_music)
    {
        if (_player.Stream == p_music) return;

        _player.Stream = p_music;
        _player.Play();
    }
}
