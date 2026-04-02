using System.Collections.Generic;
using Core.Utils;

namespace Core.Interfaces.Stats;

public interface IStatTracker
{
    void InitializeStats(Dictionary<Managers.Stats.StatType, float> p_baseStats);
    float GetCurrentValue(Managers.Stats.StatType p_statType);
    float GetEffectiveMaxValue(Managers.Stats.StatType p_statType);
    void ModifyCurrentValue(Managers.Stats.StatType p_statType, float p_amount);
    void SetCurrentValue(Managers.Stats.StatType p_statType, float p_value);
    void AddPermanentBonus(Managers.Stats.StatType p_statType, float p_amount);

    WeakEvent<Managers.Stats.StatChangedEventArgs> OnAnyStatChanged { get; }
}
