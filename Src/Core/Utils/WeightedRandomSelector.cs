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
    private readonly Random m_random;

    public WeightedRandomSelector()
    {
        m_random = new Random();
    }

    /// <inheritdoc />
    public T? SelectRandom(IEnumerable<T> p_items)
    {
        if (p_items == null)
        {
            return default;
        }

        var itemList = p_items.ToList();
        if (itemList.Count == 0)
        {
            return default;
        }

        float totalWeight = itemList.Sum(item => item.Weight);
        if (totalWeight <= 0)
        {
            // If all weights are 0, pick one randomly with equal probability
            int index = m_random.Next(itemList.Count);
            return itemList[index];
        }

        double roll = m_random.NextDouble() * totalWeight;
        float currentWeightSum = 0;

        foreach (var item in itemList)
        {
            currentWeightSum += item.Weight;
            if (roll < currentWeightSum)
            {
                return item;
            }
        }

        return itemList.Last();
    }
}
