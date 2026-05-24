using System.Collections.Generic;
using Core.Utils;

namespace Core.Interfaces.Stats;

public interface IStatTracker
{
    bool IsInitialized { get; }
    void InitializeStats(Dictionary<Managers.Stats.StatType, float> p_baseStats);
    float GetCurrentValue(Managers.Stats.StatType p_statType);
    float GetEffectiveMaxValue(Managers.Stats.StatType p_statType);
    void ModifyCurrentValue(Managers.Stats.StatType p_statType, float p_amount);
    void SetCurrentValue(Managers.Stats.StatType p_statType, float p_value);
    void AddPermanentBonus(Managers.Stats.StatType p_statType, float p_amount);
    void AddExperience(float p_amount);
    float CalculateRequiredXp(int p_level);
    void ResetStats();
}
