using Godot;

namespace IslandSurvivor.Logic;

public static class CombatMath
{
    public static int CalculateDamage(float p_baseAttack, float p_attackStat)
    {
        // Final Damage = Base Damage * (1f + (Attack * 0.05f))
        float finalDamageFloat = p_baseAttack * (1f + (p_attackStat * 0.05f));
        return Mathf.RoundToInt(finalDamageFloat);
    }

    public static float CalculateTime(float p_baseTime, float p_speedStat, float p_minTime = 0.1f)
    {
        // Cooldown/Duration = BaseTime / (1 + (Speed * 0.05))
        float calculatedTime = p_baseTime / (1f + (p_speedStat * 0.05f));
        return Mathf.Max(calculatedTime, p_minTime);
    }
}
