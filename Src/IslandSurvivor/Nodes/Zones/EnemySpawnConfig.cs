using Godot;

namespace IslandSurvivor.Nodes.Zones;

/// <summary>
/// Configuration for a specific enemy type in an EnemySpawnZone.
/// </summary>
[GlobalClass]
public partial class EnemySpawnConfig : Resource
{
    [Export] public PackedScene EnemyScene { get; set; } = null!;
    [Export] public int Count { get; set; } = 1;
}
