using System.Collections.Generic;

namespace Core.Interfaces.Utils;

/// <summary>
/// Interface for selecting a random item from a collection.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public interface IRandomSelector<T>
{
    /// <summary>
    /// Selects a random item from the provided collection.
    /// </summary>
    /// <param name="p_items">The collection of items to choose from.</param>
    /// <returns>A randomly selected item, or the default value if the collection is empty.</returns>
    T? SelectRandom(IEnumerable<T> p_items);
}
