using System.Collections.Generic;
using Core.Events;
using Core.Interfaces;
using Core.Interfaces.Stats;

namespace Core.Managers.Stats;

public class StatTracker : IStatTracker
{
    private readonly Dictionary<StatType, IStat> m_stats;
    private readonly IEventBus m_eventBus;

    public StatTracker(IEventBus p_eventBus)
    {
        m_stats = new Dictionary<StatType, IStat>();
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

    public void InitializeStats(Dictionary<StatType, float> p_baseStats)
    {
        if (p_baseStats == null) return;

        m_stats.Clear();
        foreach (KeyValuePair<StatType, float> kvp in p_baseStats)
        {
            IStat newStat;
            if (kvp.Key == StatType.Health)
            {
                newStat = new PoolStat(kvp.Key, kvp.Value, m_eventBus);
            }
            else
            {
                newStat = new AttributeStat(kvp.Key, kvp.Value, m_eventBus);
            }

            m_stats.Add(kvp.Key, newStat);
        }
    }

    public float GetCurrentValue(StatType p_statType)
    {
        if (m_stats.TryGetValue(p_statType, out IStat? stat))
        {
            return stat.CurrentValue;
        }
        return 0f;
    }

    public float GetEffectiveMaxValue(StatType p_statType)
    {
        if (m_stats.TryGetValue(p_statType, out IStat? stat))
        {
            return stat.EffectiveMaxValue;
        }
        return 0f;
    }

    public void ModifyCurrentValue(StatType p_statType, float p_amount)
    {
        if (m_stats.TryGetValue(p_statType, out IStat? stat))
        {
            stat.ModifyCurrentValue(p_amount);
        }
    }

    public void SetCurrentValue(StatType p_statType, float p_value)
    {
        if (m_stats.TryGetValue(p_statType, out IStat? stat))
        {
            stat.SetCurrentValue(p_value);
        }
    }

    public void AddPermanentBonus(StatType p_statType, float p_amount)
    {
        if (m_stats.TryGetValue(p_statType, out IStat? stat))
        {
            stat.AddBonus(p_amount);
        }
    }
}
