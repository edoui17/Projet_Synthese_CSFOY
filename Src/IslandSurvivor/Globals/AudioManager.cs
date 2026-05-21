using Godot;
using System.Collections.Generic;

namespace IslandSurvivor.Globals;

public partial class AudioManager : Node
{
    private static AudioManager m_instance;
    public static AudioManager Instance => m_instance;

    private readonly List<AudioStreamPlayer> m_availablePlayers = new();
    private readonly List<AudioStreamPlayer2D> m_availablePlayers2D = new();

    private const int POOL_SIZE = 10;
    private const int POOL_SIZE_2D = 10;

    public override void _EnterTree()
    {
        if (m_instance != null)
        {
            QueueFree();
            return;
        }
        m_instance = this;
        ProcessMode = ProcessModeEnum.Always;

        // Initialize Global SFX Pool
        for (int i = 0; i < POOL_SIZE; i++)
        {
            var player = new AudioStreamPlayer();
            AddChild(player);
            m_availablePlayers.Add(player);
            player.Finished += () => OnPlayerFinished(player);
        }

        // Initialize 2D SFX Pool
        for (int i = 0; i < POOL_SIZE_2D; i++)
        {
            var player2D = new AudioStreamPlayer2D();
            AddChild(player2D);
            m_availablePlayers2D.Add(player2D);
            player2D.Finished += () => OnPlayer2DFinished(player2D);
        }
    }

    /// <summary>
    /// Plays a global sound (e.g., UI, Upgrades)
    /// </summary>
    public void PlaySound(AudioStream p_stream, float p_volumeDb = 0f, float p_pitchScale = 1f)
    {
        if (p_stream == null) return;

        foreach (var player in m_availablePlayers)
        {
            if (!player.Playing)
            {
                player.Stream = p_stream;
                player.VolumeDb = p_volumeDb;
                player.PitchScale = p_pitchScale;
                player.Play();
                return;
            }
        }
        GD.PushWarning("AudioManager: No available global AudioStreamPlayers.");
    }

    /// <summary>
    /// Plays a spatial sound at a specific position (e.g., Impact, Death)
    /// </summary>
    public void PlaySound2D(AudioStream p_stream, Vector2 p_globalPosition, float p_volumeDb = 0f, float p_pitchScale = 1f)
    {
        if (p_stream == null) return;

        foreach (var player in m_availablePlayers2D)
        {
            if (!player.Playing)
            {
                player.Stream = p_stream;
                player.GlobalPosition = p_globalPosition;
                player.VolumeDb = p_volumeDb;
                player.PitchScale = p_pitchScale;
                player.Play();
                return;
            }
        }
        GD.PushWarning("AudioManager: No available 2D AudioStreamPlayers.");
    }

    private void OnPlayerFinished(AudioStreamPlayer p_player)
    {
        p_player.Stop();
        p_player.Stream = null;
    }

    private void OnPlayer2DFinished(AudioStreamPlayer2D p_player)
    {
        p_player.Stop();
        p_player.Stream = null;
    }
}
