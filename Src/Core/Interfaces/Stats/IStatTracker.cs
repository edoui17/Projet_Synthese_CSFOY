using System.Collections.Generic;
using Core.Utils;

namespace Core.Interfaces;

public interface IStatTracker
{
    bool IsInitialized { get; }
    void InitializeStats(Dictionary<Managers.StatType, float> p_baseStats);
    float GetCurrentValue(Managers.StatType p_statType);
    float GetEffectiveMaxValue(Managers.StatType p_statType);
    void ModifyCurrentValue(Managers.StatType p_statType, float p_amount);
    void SetCurrentValue(Managers.StatType p_statType, float p_value);
    void AddSessionBonus(Managers.StatType p_statType, float p_amount);
    void AddExperience(float p_amount);
    float CalculateRequiredXp(int p_level);
    void ResetStats();
}
