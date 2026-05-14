using Godot;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class ProgressionRequirement : Resource
{
    [Export]
    public int BossLevelRequirement { get; set; } = 10;
}
