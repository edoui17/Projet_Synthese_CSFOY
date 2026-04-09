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

    public void PlayMusic(AudioStream music)
    {
        if (_player.Stream == music) return;

        _player.Stream = music;
        _player.Play();
    }
}
