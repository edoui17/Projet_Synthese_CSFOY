namespace IslandSurvivor.Logic.StateMachine;

using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class StateMachine : State
{
    [Export] public State InitialState { get; set; } = null!;

    [ExportGroup("Combat Configuration")]
    [Export] public float AttackRange { get; set; } = 60.0f;
    [Export] public float GuardChance { get; set; } = 0.3f;

    private Dictionary<StringName, State> m_states = new Dictionary<StringName, State>();
    private State? m_currentState;

    public State? CurrentState => m_currentState;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
    }

    public override void Initialize(StateMachine p_parentMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_parentMachine, p_npcContext);

        m_states.Clear();
        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                m_states[new StringName(child.Name)] = state;
                state.Initialize(this, p_npcContext);
                state.StateFinished += OnStateFinished;
            }
        }

        if (InitialState != null && m_states.ContainsValue(InitialState))
        {
            m_currentState = InitialState;
            m_currentState.Enter();
        }
        else if (m_states.Count > 0)
        {
            // Fallback to first state if no initial state is explicitly set
            var enumerator = m_states.GetEnumerator();
            enumerator.MoveNext();
            m_currentState = enumerator.Current.Value;
            m_currentState.Enter();
        }

        if (NpcContext != null && Engine.IsEditorHint() == false)
        {
            GD.Print($"[Frame: {Engine.GetPhysicsFrames()}] [{NpcContext.Name}] [StateMachine] INITIALIZED. Total States: {m_states.Count}. Starting State: {m_currentState?.Name}");
        }
    }

    public override void Enter()
    {
        m_currentState?.Enter();
    }

    public override void Exit()
    {
        if (m_currentState != null)
        {
            m_currentState.Exit();
            m_currentState = null;
        }
    }

    public override void Update(double p_delta)
    {
        m_currentState?.Update(p_delta);
    }

    public override void PhysicsUpdate(double p_delta)
    {
        m_currentState?.PhysicsUpdate(p_delta);
    }

    private void OnStateFinished(State p_sourceState, StateExitReason p_reason)
    {
        if (p_sourceState != m_currentState) return;

        StringName nextStateName = new StringName();

        switch (p_sourceState.Name.ToString())
        {
            case "IdleState":
                if (p_reason == StateExitReason.TargetDetected)
                {
                    nextStateName = new StringName("ChaseState");
                }
                else if (p_reason == StateExitReason.CooldownFinished)
                {
                    nextStateName = GetCombatDecisionState();
                }
                break;

            case "ChaseState":
                if (p_reason == StateExitReason.TargetLost)
                {
                    nextStateName = new StringName("IdleState");
                }
                else if (p_reason == StateExitReason.TargetReached)
                {
                    nextStateName = GetCombatDecisionState();
                }
                break;

            case "AttackState":
                if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
                {
                    var target = aggNpc.GetTarget();
                    if (target != null)
                    {
                        float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
                        if (distSquared <= AttackRange * AttackRange)
                        {
                            nextStateName = GetCombatDecisionState();
                        }
                        else
                        {
                            nextStateName = new StringName("ChaseState");
                        }
                    }
                    else
                    {
                        nextStateName = new StringName("ChaseState");
                    }
                }
                else
                {
                    nextStateName = new StringName("ChaseState");
                }
                break;

            case "WindUpState":
                if (p_reason == StateExitReason.Finished)
                {
                    nextStateName = new StringName("DashState"); // Or "AttackState", depending on the NPC. Lancer uses DashState after WindUp. Wait, this needs to be specific.
                    // For the Lancer, WindUp goes to DashState.
                    // Let's assume it goes to AttackState by default, but if it has DashState, it goes to DashState?
                    // Let me check if DashState is present.
                    if (m_states.ContainsKey(new StringName("DashState")))
                        nextStateName = new StringName("DashState");
                    else
                        nextStateName = new StringName("AttackState");
                }
                break;

            case "DashState":
                nextStateName = new StringName("RecoveryState");
                break;

            case "RecoveryState":
                nextStateName = new StringName("IdleState");
                break;

            case "FleeState":
                if (p_reason == StateExitReason.Finished)
                {
                    nextStateName = new StringName("IdleState");
                }
                break;

            case "GuardState":
                nextStateName = GetPostGuardState();
                break;

            case "DeathState":
                // No transitions out of DeathState
                return;
        }

        if (!nextStateName.IsEmpty && m_states.ContainsKey(nextStateName))
        {
            if (NpcContext != null && Engine.IsEditorHint() == false)
            {
                GD.Print($"[Frame: {Engine.GetPhysicsFrames()}] [{NpcContext.Name}] [StateMachine] TRANSITION: {p_sourceState.Name} -> {nextStateName}");
            }
            ForceTransition(nextStateName.ToString());
        }
        else if (!nextStateName.IsEmpty)
        {
            if (NpcContext != null && Engine.IsEditorHint() == false)
            {
                GD.PushWarning($"[Frame: {Engine.GetPhysicsFrames()}] [{NpcContext.Name}] [StateMachine] WARNING: Attempted to transition to unknown state '{nextStateName}'");
            }
        }
    }

    private StringName GetPostGuardState()
    {
        if (NpcContext is not IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpcGuard)
            return new StringName("ChaseState");

        var target = aggNpcGuard.GetTarget();
        if (target == null)
            return new StringName("IdleState");

        float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
        if (distSquared <= AttackRange * AttackRange)
            return GetCombatDecisionState();

        return new StringName("ChaseState");
    }

    private StringName GetCombatDecisionState()
    {
        var attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");

        if (attackController != null && attackController.CanAttack)
        {
            if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggressiveNpc && aggressiveNpc.CanGuard && GD.Randf() <= GuardChance)
            {
                return new StringName("GuardState");
            }

            return new StringName("AttackState");
        }
        else
        {
            if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc && aggNpc.CanGuard)
            {
                return new StringName("GuardState");
            }
            else
            {
                return new StringName("IdleState");
            }
        }
    }

    public void ForceTransition(string p_targetStateName)
    {
        if (!m_states.ContainsKey(new StringName(p_targetStateName))) return;

        m_currentState?.Exit();
        m_currentState = m_states[new StringName(p_targetStateName)];
        m_currentState.Enter();
    }
}
