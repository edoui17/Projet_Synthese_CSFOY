namespace Core.Interfaces;

using System;
using Core.Utils;

public interface IScoreTracker
{
    int CurrentScore { get; }
    int HighScore { get; }
    int MapCount { get; }
    string CharacterId { get; }
    string CurrentIslandId { get; }

    Core.Domain.SessionState GetSessionState();
    void UpdateCurrentIsland(string p_islandId);

    void Initialize(int p_initialScore, int p_mapCount, string p_characterId);
    void AddScore(int p_amount);
    void UpdateHighScore();
    void ResetSession();
}
