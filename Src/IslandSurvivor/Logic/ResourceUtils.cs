using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Nodes;

namespace IslandSurvivor.Logic;

public static class ResourceUtils
{
    public static int CalculateYield(int baseQuantity, object? p_attacker)
    {
        float luck = 0f;
        if (p_attacker is Node GodotAttacker)
        {
            StatManager attackerStats = GodotAttacker.GetNodeOrNull<StatManager>("StatManager");
            if (attackerStats != null)
            {
                luck = attackerStats.GetCurrentValue(StatType.Luck);
            }
        }

        float bonusChance = luck * 0.05f;
        int bonusQuantity = (int)bonusChance;
        float fractionalChance = bonusChance - bonusQuantity;

        if (GD.Randf() < fractionalChance)
        {
            bonusQuantity++;
        }

        return baseQuantity + bonusQuantity;
    }
}
