namespace IslandSurvivor.Scenes.NPC;

using Godot;

public partial class Lancer : MeleeAggressiveNpcBase
{
    public override void _Ready()
    {
        base._Ready();

        // Register dash hitboxes if needed, though DashState or AttackState usually handles this.
        // Actually, DashState handles the hitboxes dynamically.
        // The Lancer no longer uses LancerController and purely relies on its StateMachine.
    }
}
