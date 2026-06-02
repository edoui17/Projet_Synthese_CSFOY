namespace IslandSurvivor.Scenes.NPC;

using Godot;

public partial class Soldier : MeleeAggressiveNpcBase
{
    // The base class handles all the standard logic for Soldier:
    // Movement, detection, hitbox attacks, death, stat scaling.

    public override void _Ready()
    {
        AttackSoundKey = "Soldier_Attack";
        Stats = GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");
        if (Stats == null)
        {
            GD.PrintErr("Soldier node requires a StatManager child node.");
        }

        base._Ready();
    }
}
