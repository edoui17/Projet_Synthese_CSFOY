using Core.Domain;

namespace Core.Utils;

public static class ProfileValidator
{
    private const float MIN_HEALTH = 0.01f;
    private const float MAX_HEALTH = 100.0f;
    private const float MIN_STAT = 0.0f;
    private const float MAX_STAT = 999.0f;

    public static bool IsValid(ProfileResponse? p_profile)
    {
        if (p_profile == null) return false;

        // Basic Health check
        // In the database it might be stored in the LastSessions.
        // We should check the stats in LastSessions as well if they are used for initialization.

        if (p_profile.LastSessions != null)
        {
            foreach (var session in p_profile.LastSessions)
            {
                if (!IsValidStats(session))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool IsValidStats(GameStats p_stats)
    {
        if (p_stats.Health <= MIN_HEALTH || p_stats.Health > MAX_HEALTH) return false;

        if (p_stats.Attack < MIN_STAT || p_stats.Attack >= MAX_STAT) return false;
        if (p_stats.Speed < MIN_STAT || p_stats.Speed >= MAX_STAT) return false;
        if (p_stats.Luck < MIN_STAT || p_stats.Luck >= MAX_STAT) return false;

        if (p_stats.BonusHealth < MIN_STAT || p_stats.BonusHealth >= MAX_STAT) return false;
        if (p_stats.BonusAttack < MIN_STAT || p_stats.BonusAttack >= MAX_STAT) return false;
        if (p_stats.BonusSpeed < MIN_STAT || p_stats.BonusSpeed >= MAX_STAT) return false;
        if (p_stats.BonusLuck < MIN_STAT || p_stats.BonusLuck >= MAX_STAT) return false;

        return true;
    }
}
