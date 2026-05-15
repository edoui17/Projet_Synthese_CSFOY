namespace Core.Services;

using Core.Domain;
using Core.Interfaces;
using System;

public class ProgressionService : IProgressionService
{
    public int CalculateLevel(int p_score)
    {
        if (p_score < 0) return 1;
        if (p_score < 1000) return 1;
        if (p_score < 2500) return 2;
        if (p_score < 5000) return 3;

        return 4 + (int)Math.Floor((p_score - 5000) / 5000.0);
    }

    public bool CheckAndUpdateHighScore(Player p_player, int p_sessionScore)
    {
        if (p_sessionScore > p_player.HighScore)
        {
            p_player.HighScore = p_sessionScore;
            return true;
        }
        return false;
    }
}
