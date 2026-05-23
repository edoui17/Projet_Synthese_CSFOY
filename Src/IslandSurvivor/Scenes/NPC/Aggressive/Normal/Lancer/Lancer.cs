namespace IslandSurvivor.Scenes.NPC.Aggressive.Normal.Lancer;

using Godot;
using IslandSurvivor.Globals;
using IslandSurvivor.Logic.Entities;

public partial class Lancer : MeleeAggressiveNpcBase
{
    [Export] public float DashSpeedMultiplier { get; set; } = 3.0f;
    [Export] public float MinDashDistance { get; set; } = 150.0f;

    public override void _Ready()
    {
        base._Ready();

        // Register dash hitboxes if needed, though DashState or AttackState usually handles this.
        // Actually, DashState handles the hitboxes dynamically.
        // The Lancer no longer uses LancerController and purely relies on its StateMachine.
    }
}
