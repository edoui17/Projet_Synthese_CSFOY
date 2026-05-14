using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces.Utils;

namespace Core.Utils;

/// <summary>
/// Implementation of IRandomSelector using System.Random.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public class RandomSelector<T> : IRandomSelector<T>
{
    private readonly Random m_random;

    public RandomSelector()
    {
        m_random = new Random();
    }

    /// <inheritdoc />
    public T? SelectRandom(IEnumerable<T> p_items)
    {
        if (p_items == null || !p_items.Any())
        {
            return default;
        }

        var list = p_items.ToList();
        int index = m_random.Next(list.Count);
        return list[index];
    }
}
