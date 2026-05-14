using System.Collections.Generic;

namespace Core.Interfaces.Utils;

/// <summary>
/// Interface for items that have a weight for random selection.
/// </summary>
public interface IWeightedItem
{
    float Weight { get; }
}

/// <summary>
/// Interface for selecting a random item from a collection based on weights.
/// </summary>
/// <typeparam name="T">The type of items in the collection, must implement IWeightedItem.</typeparam>
public interface IWeightedRandomSelector<T> where T : IWeightedItem
{
    /// <summary>
    /// Selects a random item from the provided collection based on their weights.
    /// </summary>
    /// <param name="p_items">The collection of weighted items to choose from.</param>
    /// <returns>A randomly selected item, or the default value if the collection is empty.</returns>
    T? SelectRandom(IEnumerable<T> p_items);
}
