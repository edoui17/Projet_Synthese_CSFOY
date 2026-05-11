using System;
using Core.Utils;

namespace Core.Managers.Stats;

public class PoolStat : IStat
{
    private readonly StatType m_statType;
    private float m_baseValue;
    private float m_additionalValue;
    private float m_currentValue;
    private readonly WeakEvent<StatChangedEventArgs> m_onStatChanged;

    public PoolStat(StatType p_statType, float p_baseValue)
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

        // When the max value changes (e.g. from 100 to 150),
        // the current value heals by the exact same amount (+50)
        // so 50/100 HP becomes 100/150 HP.
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
