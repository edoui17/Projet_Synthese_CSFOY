using Godot;
using System;
using Core.Domain.Models;
using Core.Interfaces;

namespace IslandSurvivor.Managers;

/// <summary>
/// [Gameplay][Système][Statistique]
/// Autoload Singleton managing the dynamic difficulty scaling of the game.
/// Calculates threat score based on Time, Player Level, and Island Biome type.
/// </summary>
public partial class DifficultyManager : Node, IDifficultyManager
{
    public static DifficultyManager Instance { get; private set; } = null!;

    private float m_timeElapsed = 0f;
    public float TimeElapsed => m_timeElapsed;

    private float m_islandMultiplier = 1.0f;
    private const float MAX_THREAT_SCORE = 6.0f;
    private const float MAX_TIME_MINUTES = 30f;
    private const float SECONDS_PER_MINUTE = 60f;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public override void _Process(double p_delta)
    {
        m_timeElapsed += (float)p_delta;
    }

    /// <summary>
    /// Sets the island multiplier based on the given island difficulty.
    /// Poor = 0.8x
    /// Normal = 1.0x
    /// Hard = 1.5x
    /// </summary>
    public void SetIslandDifficulty(IslandDifficulty p_difficulty)
    {
        m_islandMultiplier = p_difficulty switch
        {
            IslandDifficulty.Poor => 0.8f,
            IslandDifficulty.Normal => 1.0f,
            IslandDifficulty.Hard => 1.5f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Calculates the Global Threat Score based on Time, Player Level, and Island Type.
    /// Uses exponential growth for level scaling to keep difficulty manageable early but steep later.
    /// Formula: TimeScore * LevelScore * IslandScore
    /// </summary>
    public float GetGlobalThreatScore(int p_playerLevel)
    {
        // Linear increase over 30 minutes, max +2.0 bonus
        float timeFactor = m_timeElapsed / (MAX_TIME_MINUTES * SECONDS_PER_MINUTE);
        float timeMultiplier = 1.0f + Mathf.Clamp(timeFactor, 0f, 1f) * 2.0f;

        // Compound growth per level: +15% per level, starting at 1.0 at Level 1.
        float levelMultiplier = (float)Math.Pow(1.15, Math.Max(p_playerLevel - 1, 0));

        float totalThreat = timeMultiplier * levelMultiplier * m_islandMultiplier;

        return Mathf.Clamp(totalThreat, 1.0f, MAX_THREAT_SCORE);
    }

    /// <summary>
    /// Resets the elapsed time, usually called when transitioning between islands.
    /// </summary>
    public void ResetTime()
    {
        m_timeElapsed = 0f;
    }
}
