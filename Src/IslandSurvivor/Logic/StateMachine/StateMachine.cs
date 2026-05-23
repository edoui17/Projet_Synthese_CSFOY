namespace IslandSurvivor.Logic.StateMachine;

using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class StateMachine : State
{
    [Export] public State InitialState { get; set; } = null!;

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
                state.TransitionRequested += OnTransitionRequested;
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

    private void OnTransitionRequested(State p_sourceState, StringName p_targetStateName)
    {
        if (p_sourceState != m_currentState) return;

        if (!m_states.ContainsKey(p_targetStateName))
        {
            GD.PushWarning($"StateMachine '{Name}' trying to transition to unknown state: {p_targetStateName}");
            return;
        }

        m_currentState?.Exit();
        m_currentState = m_states[p_targetStateName];
        m_currentState.Enter();
    }

    public void ForceTransition(string p_targetStateName)
    {
        if (!m_states.ContainsKey(new StringName(p_targetStateName))) return;

        m_currentState?.Exit();
        m_currentState = m_states[new StringName(p_targetStateName)];
        m_currentState.Enter();
    }
}
