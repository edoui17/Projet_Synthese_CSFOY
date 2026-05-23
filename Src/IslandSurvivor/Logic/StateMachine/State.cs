namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class State : Node
{
    [Signal]
    public delegate void TransitionRequestedEventHandler(State p_sourceState, StringName p_targetStateName);

    public StateMachine StateMachine { get; protected set; }
    public CharacterBody2D NpcContext { get; protected set; }

    public virtual void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        StateMachine = p_stateMachine;
        NpcContext = p_npcContext;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(double p_delta) { }
    public virtual void PhysicsUpdate(double p_delta) { }

    protected void TransitionTo(string p_targetStateName)
    {
        EmitSignal(SignalName.TransitionRequested, this, new StringName(p_targetStateName));
    }
}
