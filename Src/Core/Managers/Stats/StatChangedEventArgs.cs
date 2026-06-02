using System;

namespace Core.Managers;

public class StatChangedEventArgs : EventArgs
{
    private readonly StatType m_statType;
    private readonly float m_currentValue;
    private readonly float m_effectiveMaxValue;

    public StatChangedEventArgs(StatType p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        m_statType = p_statType;
        m_currentValue = p_currentValue;
        m_effectiveMaxValue = p_effectiveMaxValue;
    }

    public StatType StatType => m_statType;
    public float CurrentValue => m_currentValue;
    public float EffectiveMaxValue => m_effectiveMaxValue;
}
