namespace IslandSurvivor.Scenes.NPC.Agressive;

using Godot;

public partial class Archer : RangedEnemyBase
{
    public override void _Ready()
    {
        Stats = GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");
        if (Stats == null)
        {
            GD.PrintErr("Archer node requires a StatManager child node.");
        }

        // Load the Arrow scene
        ProjectileScene = GD.Load<PackedScene>("res://Scenes/Projectiles/Arrow.tscn");
        if (ProjectileScene == null)
        {
            GD.PrintErr("Archer failed to load Arrow.tscn!");
        }

        base._Ready();
    }
}
