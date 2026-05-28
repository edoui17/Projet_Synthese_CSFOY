using Core.Domain.Models;

namespace Core.Interfaces;

public interface IDifficultyManager
{
    float TimeElapsed { get; }
    void SetIslandDifficulty(IslandDifficulty p_difficulty);
    float GetGlobalThreatScore(int p_playerLevel);
    void ResetTime();
}
