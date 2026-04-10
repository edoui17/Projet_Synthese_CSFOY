namespace Core.Interfaces.Stats;

using System;
using Core.Utils;

public interface IScoreTracker
{
    public class ScoreChangedEventArgs : EventArgs
    {
        public int PreviousScore { get; }
        public int NewScore { get; }

        public ScoreChangedEventArgs(int p_previousScore, int p_newScore)
        {
            PreviousScore = p_previousScore;
            NewScore = p_newScore;
        }
    }

    WeakEvent<ScoreChangedEventArgs> OnScoreChanged { get; }

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
}
