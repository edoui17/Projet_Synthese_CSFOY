using Core.Utils;

namespace Core.Managers.Stats;

public interface IStat
{
    StatType StatType { get; }
    float BaseValue { get; }
    float AdditionalValue { get; }
    float EffectiveMaxValue { get; }
    float CurrentValue { get; }
    WeakEvent<StatChangedEventArgs> OnStatChanged { get; }

    void AddBonus(float p_amount);
    void ModifyCurrentValue(float p_amount);
    void SetCurrentValue(float p_value);
}