using System;
using Core.Utils;

namespace Core.Managers.Stats;

public class AttributeStat : IStat
{
    private readonly StatType m_statType;
    private float m_baseValue;
    private float m_additionalValue;
    private readonly WeakEvent<StatChangedEventArgs> m_onStatChanged;

    public AttributeStat(StatType p_statType, float p_baseValue)
    {
        m_statType = p_statType;
        m_baseValue = p_baseValue;
        m_additionalValue = 0f;
        m_onStatChanged = new WeakEvent<StatChangedEventArgs>();
    }

    public StatType StatType => m_statType;
    public float BaseValue => m_baseValue;
    public float AdditionalValue => m_additionalValue;
    public float EffectiveMaxValue => m_baseValue + m_additionalValue;
    public float CurrentValue => EffectiveMaxValue; // Attributes do not have a separate current value
    public WeakEvent<StatChangedEventArgs> OnStatChanged => m_onStatChanged;

    public void AddBonus(float p_amount)
    {
        m_additionalValue += p_amount;
        NotifyStatChanged();
    }

    public void ModifyCurrentValue(float p_amount)
    {
        // For attribute stats, we don't have a temporary current value to modify.
        // If needed, this could throw an exception or be a no-op.
        // For now, let's treat it as a temporary bonus/penalty, but it's better to just ignore or throw if not supported.
        // Since IStat requires it, we'll no-op or throw. No-op is safer for now.
    }

    public void SetCurrentValue(float p_value)
    {
        // Similar to ModifyCurrentValue, attributes don't have a separate current value.
    }

    private void NotifyStatChanged()
    {
        m_onStatChanged.Invoke(this, new StatChangedEventArgs(m_statType, CurrentValue, EffectiveMaxValue));
    }
}