using Godot;

namespace IslandSurvivor.Scenes.NPC;

public partial class Reaper : BossBase
{
    [ExportGroup("Reaper Animations")]
    [Export] public string NormalAttackAnimationName { get; set; } = "MeleeAttackNormal";
    [Export] public string EnragedAttackAnimationName { get; set; } = "MeleeAttackEnraged";

}
