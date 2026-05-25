using Godot;
using Core.Domain.Models;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class IslandConfig : Resource
{
    [Export] public IslandDifficulty DifficultyLevel { get; set; } = IslandDifficulty.Normal;
    [Export] public string BiomeName { get; set; } = "Unknown";
}
