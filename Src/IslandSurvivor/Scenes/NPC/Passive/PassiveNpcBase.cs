namespace IslandSurvivor.Scenes.NPC.Passive;

using Godot;
using IslandSurvivor.Logic.Entities;

public partial class PassiveNpcBase : NpcBase
{
    [Export] public float FleeSpeed { get; set; } = 120.0f;

    protected PassiveController m_passiveController = null!;

    public override string CurrentState => m_passiveController?.CurrentState ?? NpcStates.IDLE;

    public override void _Ready()
    {
        base._Ready();
        m_passiveController = new PassiveController();
        NpcType = "Passive";
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_passiveController.CurrentState == NpcStates.DEAD) return;

        m_passiveController.Update((float)p_delta);

        Vector2 direction = m_passiveController.CurrentDirection;
        float targetSpeed = IdleSpeed;

        if (m_passiveController.CurrentState == NpcStates.FLEE)
        {
            targetSpeed = FleeSpeed;
        }

        if (m_movementController != null)
        {
            m_movementController.Move(direction, targetSpeed);
        }
        else
        {
            Velocity = direction * targetSpeed;
            MoveAndSlide();
        }

        if (m_passiveController.CurrentState == NpcStates.IDLE && GetSlideCollisionCount() > 0)
        {
            m_passiveController.ForceNewDirection();
        }

    }

    protected override void OnDamageTaken(Node2D p_attacker)
    {
        base.OnDamageTaken(p_attacker);
        m_passiveController.StartFleeing(GlobalPosition, p_attacker.GlobalPosition);
    }
}