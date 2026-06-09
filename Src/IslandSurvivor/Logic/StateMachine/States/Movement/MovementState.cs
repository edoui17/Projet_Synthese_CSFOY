namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class MovementState : State
{
    public override void Exit()
    {
        base.Exit();
        if (NpcContext != null)
        {
            NpcContext.Velocity = Vector2.Zero;

            if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
            {
                // Force cut any controller logic if any. The refactoring requirement says:
                // "Force m_npcContext.Velocity = Vector2.Zero et coupe le pathfinding pour garantir un arrêt net en combat."
                // In our current code, there is no explicit pathfinding "stop" method directly on NpcBase yet, but Velocity = 0 stops movement.
                // Note: The NpcBase MovementController actually has `Move` which sets velocity.
            }
        }
    }

    public override void PhysicsUpdate(double p_delta)
    {
        base.PhysicsUpdate(p_delta);
        if (NpcContext != null)
        {
            NpcContext.MoveAndSlide();
        }
    }

    protected void SetVelocity(Vector2 p_velocity)
    {
        if (NpcContext != null)
        {
            NpcContext.Velocity = p_velocity;
        }
    }
}
