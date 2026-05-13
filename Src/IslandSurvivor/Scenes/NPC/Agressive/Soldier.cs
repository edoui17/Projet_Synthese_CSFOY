namespace IslandSurvivor.Scenes.NPC.Agressive;

using Godot;

public partial class Soldier : EnemyBase
{
    // The base class EnemyBase handles all the standard logic for Soldier:
    // Movement, detection, hitbox attacks, death, stat scaling.

    public override void _Ready()
    {
        Stats = GetNodeOrNull<IslandSurvivor.Nodes.StatManager>("StatManager");
        if (Stats == null)
        {
            GD.PrintErr("Soldier node requires a StatManager child node.");
        }

        base._Ready();
    }
}
