namespace IslandSurvivor.Scenes.NPC;

using Godot;
using IslandSurvivor.Logic;

public partial class PassiveNpcBase : NpcBase
{

    public override Godot.StringName GetDecisionState(Node2D target)
    {
        if (target != null)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.FleeStateName;
        }
        return IslandSurvivor.Logic.StateMachine.StateConstants.WanderStateName;
    }

    public override void _Ready()
    {
        base._Ready();
        NpcType = "Passive";
    }

    protected override void OnDamageTaken(Node2D p_attacker)
    {
        base.OnDamageTaken(p_attacker);
        m_stateMachine?.ForceTransition(IslandSurvivor.Logic.StateMachine.StateConstants.FleeStateName);
    }
}