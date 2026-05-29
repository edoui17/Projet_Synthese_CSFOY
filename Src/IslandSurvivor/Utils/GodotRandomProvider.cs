using Godot;
using Core.Interfaces;

namespace IslandSurvivor.Utils;

/// <summary>
/// A Godot-specific implementation of IRandomProvider that relies on Godot's internal RandomNumberGenerator
/// to avoid C# garbage collection spikes and respect Godot's global seed.
/// </summary>
public class GodotRandomProvider : IRandomProvider
{
    public int Next(int maxValue)
    {
        // GD.RandRange returns a double. We can use GD.Randi() % maxValue
        // which exactly mimics standard Random.Next(maxValue) behavior.
        if (maxValue <= 0) return 0;
        return (int)(GD.Randi() % (uint)maxValue);
    }

    public double NextDouble()
    {
        return GD.Randf();
    }
}
