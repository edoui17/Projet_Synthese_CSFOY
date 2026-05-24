namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class State : Node
{
    [Signal]
    public delegate void StateFinishedEventHandler(State p_sourceState, StateExitReason p_reason);

    public StateMachine StateMachine { get; protected set; }
    public CharacterBody2D NpcContext { get; protected set; }

    public virtual void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        StateMachine = p_stateMachine;
        NpcContext = p_npcContext;
        // Do not spam Initialization print here, StateMachine handles it.
    }

    public virtual void Enter()
    {
        if (NpcContext != null && Engine.IsEditorHint() == false)
        {
            GD.Print($"[Frame: {Engine.GetPhysicsFrames()}] [{NpcContext.Name}] [{Name}] ENTERED");
        }
    }

    public virtual void Exit()
    {
        if (NpcContext != null && Engine.IsEditorHint() == false)
        {
            GD.Print($"[Frame: {Engine.GetPhysicsFrames()}] [{NpcContext.Name}] [{Name}] EXITED");
        }
    }

    public virtual void Update(double p_delta) { }
    public virtual void PhysicsUpdate(double p_delta) { }

    protected void CompleteState(StateExitReason p_reason)
    {
        if (NpcContext != null && Engine.IsEditorHint() == false)
        {
            GD.Print($"[Frame: {Engine.GetPhysicsFrames()}] [{NpcContext.Name}] [{Name}] COMPLETED. Reason: {p_reason}");
        }
        EmitSignal(SignalName.StateFinished, this, Variant.From(p_reason));
    }
}
