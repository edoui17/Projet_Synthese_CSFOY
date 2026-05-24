namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class LancerRepositionState : RepositionState
{
    public override void Enter()
    {
        base.Enter();

        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
        {
            var target = aggNpc.GetTarget();
            if (target != null)
            {
                // Calculate dash vector directly toward the player
                aggNpc.LockedDirection = (target.GlobalPosition - NpcContext.GlobalPosition).Normalized(); // Pre-calculate perfectly toward player
            }
            else
            {
                aggNpc.LockedDirection = Vector2.Right;
            }
        }

        // Immediately transition to DashState after calculating the vector
        CompleteState(StateExitReason.Finished);
    }

    public override void PhysicsUpdate(double p_delta)
    {
        // No-op, we transition immediately in Enter
    }
}
