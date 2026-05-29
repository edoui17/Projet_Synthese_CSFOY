namespace IslandSurvivor.Scenes.NPC;

using Godot;

public partial class Archer : RangedAggressiveNpcBase
{
    // RangedAggressiveNpcBase handles the standard ranged logic for Archer:
    // Movement, detection, shooting, death, stat scaling.

    public override void _Ready()
    {
        AttackSoundKey = "Archer_Attack";
        Stats = GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");
        if (Stats == null)
        {
            GD.PrintErr("Archer node requires a StatManager child node.");
        }

        base._Ready();
    }
}
