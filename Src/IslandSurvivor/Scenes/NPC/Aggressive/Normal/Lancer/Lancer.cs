namespace IslandSurvivor.Scenes.NPC;

using Godot;
using IslandSurvivor.Globals;
using IslandSurvivor.Logic;

public partial class Lancer : MeleeAggressiveNpcBase
{
    [Export] public float DashThreshold { get; set; } = 150.0f;


    public override void _Ready()
    {
        base._Ready();

        // Register dash hitboxes if needed, though DashState or AttackState usually handles this.
        // Actually, DashState handles the hitboxes dynamically.
        // The Lancer no longer uses LancerController and purely relies on its StateMachine.
    }
}
