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

        // Initialize Level and XP if not provided in base stats
        if (!m_stats.ContainsKey(StatType.Level))
        {
            m_stats.Add(StatType.Level, new AttributeStat(StatType.Level, 1f, m_eventBus));
        }
        if (!m_stats.ContainsKey(StatType.Experience))
        {
            m_stats.Add(StatType.Experience, new AttributeStat(StatType.Experience, 0f, m_eventBus));
        }
    }

    public void AddExperience(float p_amount)
    {
        if (p_amount <= 0) return;

        float currentXp = GetCurrentValue(StatType.Experience);
        float currentLevel = GetCurrentValue(StatType.Level);

        currentXp += p_amount;
        SetCurrentValue(StatType.Experience, currentXp);
        m_eventBus.Publish(new ExperienceGainedEvent(p_amount));

        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        float currentXp = GetCurrentValue(StatType.Experience);
        float currentLevel = GetCurrentValue(StatType.Level);

        float requiredXp = CalculateRequiredXp((int)currentLevel);

        while (currentXp >= requiredXp)
        {
            currentXp -= requiredXp;
            currentLevel += 1;

            SetCurrentValue(StatType.Experience, currentXp);
            SetCurrentValue(StatType.Level, currentLevel);

            m_eventBus.Publish(new LevelChangedEvent((int)currentLevel));

            requiredXp = CalculateRequiredXp((int)currentLevel);
        }
    }

    public float CalculateRequiredXp(int p_level)
    {
        return 50f + (p_level * 50f);
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
