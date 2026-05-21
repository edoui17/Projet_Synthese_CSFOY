using Godot;
using System.Collections.Generic;

namespace IslandSurvivor.Globals;

public partial class AudioManager : Node
{
    private class SoundData
    {
        public AudioStream Stream { get; set; } = null!;
        public float DefaultVolumeDb { get; set; } = 0f;
    }

    private static AudioManager m_instance;
    public static AudioManager Instance => m_instance;

    private readonly List<AudioStreamPlayer> m_availablePlayers = new();
    private readonly List<AudioStreamPlayer2D> m_availablePlayers2D = new();
    private readonly Dictionary<string, SoundData> m_soundLibrary = new();

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

        InitializeLibrary();
    }

    private void InitializeLibrary()
    {
        // Enemy Sounds
        RegisterSound("Enemy_Swing_Default", "res://Assets/Audio/kenney_impact-sounds/Audio/jofae-swing-whoosh-110410.mp3");
        RegisterSound("Enemy_Hurt_Default", "res://Assets/Audio/kenney_impact-sounds/Audio/impactGlass_medium_003.ogg");
        RegisterSound("Enemy_Death_Default", "res://Assets/Sounds/Combat/enemy_death.wav");

        // Player Sounds
        RegisterSound("Player_Swing", "res://Assets/Audio/kenney_impact-sounds/Audio/jofae-swing-whoosh-110410.mp3");
        RegisterSound("Player_Hurt", "res://Assets/Audio/kenney_impact-sounds/Audio/freesound_community-alphascream001-98301.mp3");
        RegisterSound("Player_Death", "res://Assets/Audio/kenney_impact-sounds/Audio/stickypix7996-bell-toll-407826.mp3");
        RegisterSound("Level_Up", "res://Assets/Audio/kenney_impact-sounds/Audio/floraphonic-cute-level-up-2-189851.mp3");

        // UI Sounds
        RegisterSound("Stat_Upgrade", "res://Assets/Sounds/UI/stat_upgrade.wav");

        // Resource Sounds
        RegisterSound("Resource_Mining_1", "res://Assets/Audio/kenney_impact-sounds/Audio/impactMining_001.ogg");
        RegisterSound("Resource_Mining_2", "res://Assets/Audio/kenney_impact-sounds/Audio/impactMining_002.ogg");
        RegisterSound("Resource_Rustling", "res://Assets/Audio/kenney_impact-sounds/Audio/floraphonic-rustling-bushes-dried-leaves-2-230202.mp3");
        RegisterSound("Impact_Wood_Light", "res://Assets/Audio/kenney_impact-sounds/Audio/impactWood_light_003.ogg");
        RegisterSound("Impact_Wood_Heavy", "res://Assets/Audio/kenney_impact-sounds/Audio/impactWood_heavy_004.ogg");

        // NPC Sounds
        RegisterSound("Sheep_Hurt", "res://Assets/Audio/kenney_impact-sounds/Audio/scottishperson-sound-effect-woman-scream-236488.mp3");
    }

    private void RegisterSound(string p_key, string p_path, float p_defaultVolumeDb = 0f)
    {
        if (FileAccess.FileExists(p_path))
        {
            m_soundLibrary[p_key] = new SoundData
            {
                Stream = GD.Load<AudioStream>(p_path),
                DefaultVolumeDb = p_defaultVolumeDb
            };
        }
        else
        {
            GD.PushWarning($"AudioManager: Sound file not found at {p_path} for key {p_key}");
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
    /// Plays a global sound by key
    /// </summary>
    public void PlaySound(string p_soundKey, float p_volumeOffsetDb = 0f, float p_pitchScale = 1f)
    {
        if (m_soundLibrary.TryGetValue(p_soundKey, out var data))
        {
            PlaySound(data.Stream, data.DefaultVolumeDb + p_volumeOffsetDb, p_pitchScale);
        }
        else
        {
            GD.PushWarning($"AudioManager: Sound key '{p_soundKey}' not found.");
        }
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

    /// <summary>
    /// Plays a spatial sound by key
    /// </summary>
    public void PlaySound2D(string p_soundKey, Vector2 p_globalPosition, float p_volumeOffsetDb = 0f, float p_pitchScale = 1f)
    {
        if (m_soundLibrary.TryGetValue(p_soundKey, out var data))
        {
            PlaySound2D(data.Stream, p_globalPosition, data.DefaultVolumeDb + p_volumeOffsetDb, p_pitchScale);
        }
        else
        {
            GD.PushWarning($"AudioManager: Sound key '{p_soundKey}' not found.");
        }
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
