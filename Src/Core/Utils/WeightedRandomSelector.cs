using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces.Utils;

namespace Core.Utils;

/// <summary>
/// Implementation of IWeightedRandomSelector using System.Random.
/// </summary>
/// <typeparam name="T">The type of items in the collection, must implement IWeightedItem.</typeparam>
public class WeightedRandomSelector<T> : IWeightedRandomSelector<T> where T : IWeightedItem
{
    private readonly IRandomProvider m_random;
    private readonly ILogger? m_logger;

    public WeightedRandomSelector(IRandomProvider p_random, ILogger? p_logger = null)
    {
        m_random = p_random ?? throw new ArgumentNullException(nameof(p_random));
        m_logger = p_logger;
    }

    /// <inheritdoc />
    public T? SelectRandom(IReadOnlyList<T> p_items)
    {
        if (p_items == null || p_items.Count == 0)
        {
            return default;
        }

        float totalWeight = 0;
        foreach (var item in p_items)
        {
            if (item.Weight < 0)
            {
                m_logger?.LogWarning($"Negative weight {item.Weight} detected in WeightedRandomSelector. Clamping to 0.");
            }
            totalWeight += Math.Max(0, item.Weight);
        }

        if (totalWeight <= 0)
        {
            // If all weights are 0, pick one randomly with equal probability
            int index = m_random.Next(p_items.Count);
            return p_items[index];
        }

        double roll = m_random.NextDouble() * totalWeight;
        float currentWeightSum = 0;

        foreach (var item in p_items)
        {
            float clampedWeight = Math.Max(0, item.Weight);
            currentWeightSum += clampedWeight;
            if (roll < currentWeightSum)
            {
                return item;
            }
        }

        return p_items[p_items.Count - 1];
    }
}
