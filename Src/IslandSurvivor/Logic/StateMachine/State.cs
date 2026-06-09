namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class State : Node
{
    [Signal]
    public delegate void StateFinishedEventHandler(State p_sourceState, StateExitReason p_reason);

    public StateMachine StateMachine { get; protected set; } = null!;
    public CharacterBody2D NpcContext { get; protected set; } = null!;

    [ExportGroup("State Animations")]
    [Export] public string FallbackAnimationName { get; set; } = "Idle";

    public virtual bool IsActionState => false;

    public virtual void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        StateMachine = p_stateMachine;
        NpcContext = p_npcContext;
        // Do not spam Initialization print here, StateMachine handles it.
    }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }

    public virtual void Update(double p_delta) { }
    public virtual void PhysicsUpdate(double p_delta) { }

    protected void CompleteState(StateExitReason p_reason)
    {
        EmitSignal(SignalName.StateFinished, this, Variant.From(p_reason));
    }
}
