using System;
using System.Collections.Generic;
using Core.Events;
using Core.Interfaces;
using Core.Interfaces.Stats;
using Core.Utils;

namespace Core.Managers.Stats;

public class StatTracker : IStatTracker
{
    private readonly Dictionary<StatType, Stat> m_stats;
    private readonly WeakEvent<StatChangedEventArgs> m_onAnyStatChanged;
    private readonly IEventBus m_eventBus;

    public StatTracker(IEventBus p_eventBus)
    {
        m_stats = new Dictionary<StatType, Stat>();
        m_onAnyStatChanged = new WeakEvent<StatChangedEventArgs>();
        m_eventBus = p_eventBus;

        m_eventBus.Subscribe<ResourceHarvestedEvent>(OnResourceHarvested);
        m_eventBus.Subscribe<ResourceSpentEvent>(OnResourceSpent);
    }

    private void OnResourceHarvested(ResourceHarvestedEvent p_event)
    {
        // Example: Logging or updating "Total Resources Harvested" stat
        // using the event bus decoupled data.
    }

    private void OnResourceSpent(ResourceSpentEvent p_event)
    {
        // Example: Logging or updating "Total Resources Spent" stat
    }

    public WeakEvent<StatChangedEventArgs> OnAnyStatChanged => m_onAnyStatChanged;

    public void InitializeStats(Dictionary<StatType, float> p_baseStats)
    {
        if (p_baseStats == null) return;

        m_stats.Clear();
        foreach (KeyValuePair<StatType, float> kvp in p_baseStats)
        {
            Stat newStat = new Stat(kvp.Key, kvp.Value);
            newStat.OnStatChanged.AddListener(OnSingleStatChanged);
            m_stats.Add(kvp.Key, newStat);
        }
    }

    public float GetCurrentValue(StatType p_statType)
    {
        if (m_stats.TryGetValue(p_statType, out Stat? stat))
        {
            return stat.CurrentValue;
        }
        return 0f;
    }

    public float GetEffectiveMaxValue(StatType p_statType)
    {
        if (m_stats.TryGetValue(p_statType, out Stat? stat))
        {
            return stat.EffectiveMaxValue;
        }
        return 0f;
    }

    public void ModifyCurrentValue(StatType p_statType, float p_amount)
    {
        if (m_stats.TryGetValue(p_statType, out Stat? stat))
        {
            stat.ModifyCurrentValue(p_amount);
        }
    }

    public void SetCurrentValue(StatType p_statType, float p_value)
    {
        if (m_stats.TryGetValue(p_statType, out Stat? stat))
        {
            stat.SetCurrentValue(p_value);
        }
    }

    public void AddPermanentBonus(StatType p_statType, float p_amount)
    {
        if (m_stats.TryGetValue(p_statType, out Stat? stat))
        {
            stat.AddBonus(p_amount);
        }
    }

    private void OnSingleStatChanged(object? p_sender, StatChangedEventArgs p_args)
    {
        m_onAnyStatChanged.Invoke(this, p_args);
    }
}
