using System;
using Core.Utils;

namespace Core.Managers.Stats;

public class Stat
{
    private readonly StatType m_statType;
    private float m_baseValue;
    private float m_additionalValue;
    private float m_currentValue;
    private readonly WeakEvent<StatChangedEventArgs> m_onStatChanged;

    public Stat(StatType p_statType, float p_baseValue)
    {
        m_statType = p_statType;
        m_baseValue = p_baseValue;
        m_additionalValue = 0f;
        m_currentValue = EffectiveMaxValue;
        m_onStatChanged = new WeakEvent<StatChangedEventArgs>();
    }

    public StatType StatType => m_statType;
    public float BaseValue => m_baseValue;
    public float AdditionalValue => m_additionalValue;
    public float EffectiveMaxValue => m_baseValue + m_additionalValue;
    public float CurrentValue => m_currentValue;
    public WeakEvent<StatChangedEventArgs> OnStatChanged => m_onStatChanged;

    public void AddBonus(float p_amount)
    {
        m_additionalValue += p_amount;

        // When max value changes, current value might need adjustment or we might want to heal proportionately,
        // but for now, we just ensure it doesn't exceed the new max.
        // Wait, if maximum health increases, typically current health increases by the same amount or stays the same.
        // Let's keep it simple: Current value is clamped to the new EffectiveMaxValue.
        // But what if it was full health?
        // Scenario 4: "Alors la valeur totale calculée doit être mise à jour immédiatement"
        // Let's adjust current value by the same amount if it's an additive bonus, so max and current increase together.
        m_currentValue += p_amount;

        ClampCurrentValue();
        NotifyStatChanged();
    }

    public void ModifyCurrentValue(float p_amount)
    {
        m_currentValue += p_amount;
        ClampCurrentValue();
        NotifyStatChanged();
    }

    public void SetCurrentValue(float p_value)
    {
        m_currentValue = p_value;
        ClampCurrentValue();
        NotifyStatChanged();
    }

    private void ClampCurrentValue()
    {
        if (m_currentValue < 0f)
        {
            m_currentValue = 0f;
        }
        else if (m_currentValue > EffectiveMaxValue)
        {
            m_currentValue = EffectiveMaxValue;
        }
    }

    private void NotifyStatChanged()
    {
        m_onStatChanged.Invoke(this, new StatChangedEventArgs(m_statType, m_currentValue, EffectiveMaxValue));
    }
}
