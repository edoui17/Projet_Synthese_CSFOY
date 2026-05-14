using Godot;
using Core.Interfaces.Utils;

namespace IslandSurvivor.Nodes.Zones;

/// <summary>
/// Configuration for a specific enemy type in an EnemySpawnZone.
/// </summary>
[GlobalClass]
public partial class EnemySpawnConfig : Resource, IWeightedItem
{
    [Export] public PackedScene EnemyScene { get; set; } = null!;

    [Export] public float Weight { get; set; } = 1.0f;
}
