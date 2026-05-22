namespace IslandSurvivor.Scenes.NPC.Aggressive;

using Godot;

public partial class Archer : RangedAggressiveNpcBase
{
    public override void _Ready()
    {
        AttackSoundKey = "Archer_Attack";
        Stats = GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");
        if (Stats == null)
        {
            GD.PrintErr("Archer node requires a StatManager child node.");
        }

        // Load the Arrow scene
        if (ProjectileScene == null)
        {
            GD.PrintErr("Archer failed to load Arrow.tscn!");
        }

        base._Ready();
    }
}
