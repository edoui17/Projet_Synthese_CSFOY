using Godot;
using Core.Interfaces;

namespace IslandSurvivor.Nodes;

/// <summary>
/// [Gameplay][Spawning]
/// Configuration for a specific resource type in a ResourceZone.
/// </summary>
[GlobalClass]
public partial class ResourceSpawnConfig : Resource, IWeightedItem
{
    [Export] public PackedScene ResourceScene { get; set; } = null!;

    [Export] public float Weight { get; set; } = 1.0f;
}
