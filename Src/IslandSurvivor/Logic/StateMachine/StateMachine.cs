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
        RegisterStatesRecursive(this, p_npcContext);

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

    private void RegisterStatesRecursive(Node p_node, CharacterBody2D p_npcContext)
    {
        foreach (Node child in p_node.GetChildren())
        {
            if (child is State state)
            {
                m_states[state.Name] = state;
                state.Initialize(this, p_npcContext);
                state.StateFinished += OnStateFinished;
            }

            // Recursively search children (for grouping nodes like AttackState)
            RegisterStatesRecursive(child, p_npcContext);
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

        StringName nextStateName = null!;

        switch (p_sourceState.Name.ToString())
        {
            case "LancerRepositionState":
                if (p_reason == StateExitReason.Finished)
                {
                    nextStateName = StateConstants.DashStateName;
                }
                break;

            case StateConstants.IdleState:
                if (p_reason == StateExitReason.TargetDetected)
                {
                    if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
                    {
                        var target = (npc as IslandSurvivor.Scenes.NPC.AggressiveNpcBase)?.GetTarget();
                        nextStateName = npc.GetDecisionState(target!);
                    }
                    else
                    {
                        nextStateName = StateConstants.ChaseStateName;
                    }
                }
                else if (p_reason == StateExitReason.CooldownFinished)
                {
                    nextStateName = GetCombatDecisionState();
                }
                else if (p_reason == StateExitReason.Finished)
                {
                    nextStateName = GetCombatDecisionState();
                }
                else if (p_reason == StateExitReason.Timeout)
                {
                    nextStateName = StateConstants.IdleStateName;
                }
                break;

            case StateConstants.ChaseState:
                if (p_reason == StateExitReason.TargetLost)
                {
                    nextStateName = StateConstants.IdleStateName;
                }
                else if (p_reason == StateExitReason.TargetReached)
                {
                    nextStateName = GetCombatDecisionState();
                }
                else if (p_reason == StateExitReason.Timeout)
                {
                    nextStateName = StateConstants.IdleStateName;
                }
                break;

            case StateConstants.MeleeAttackState:
            case StateConstants.RangedAttackState:
            case StateConstants.MagicAttackState:
                if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc)
                {
                    var target = aggNpc.GetTarget();
                    if (target != null)
                    {
                        nextStateName = StateConstants.RecoveryStateName;
                    }
                    else
                    {
                        nextStateName = StateConstants.IdleStateName;
                    }
                }
                else
                {
                    nextStateName = StateConstants.IdleStateName;
                }
                break;

            case StateConstants.WindUpState:
                if (p_reason == StateExitReason.Finished)
                {
                    if (p_sourceState is IslandSurvivor.Logic.StateMachine.WindUpState windUp)
                    {
                        nextStateName = windUp.NextStateAfterWindup;
                    }
                    else
                    {
                        nextStateName = StateConstants.MeleeAttackStateName;
                    }
                }
                break;

            case StateConstants.DashState:
                nextStateName = StateConstants.RecoveryStateName;
                break;

            case StateConstants.RecoveryState:
                nextStateName = StateConstants.IdleStateName;
                break;

            case StateConstants.FleeState:
                if (p_reason == StateExitReason.Finished)
                {
                    nextStateName = StateConstants.IdleStateName;
                }
                break;

            case StateConstants.RepositionState:
                if (p_reason == StateExitReason.Finished)
                {
                    if (NpcContext is IslandSurvivor.Scenes.NPC.Lancer lancer)
                    {
                        nextStateName = StateConstants.DashStateName;
                    }
                    else
                    {
                        nextStateName = StateConstants.IdleStateName;
                    }
                }
                break;

            case StateConstants.GuardingState:
                nextStateName = GetPostGuardingState();
                break;

            case StateConstants.DeathState:
                // No transitions out of DeathState
                return;

            case StateConstants.WanderState:
                if (p_reason == StateExitReason.TargetDetected)
                {
                    nextStateName = StateConstants.ChaseStateName;
                }
                else if (p_reason == StateExitReason.Finished)
                {
                    nextStateName = StateConstants.IdleStateName;
                }
                break;
        }

        if (nextStateName != null && m_states.ContainsKey(nextStateName))
        {
            if (NpcContext != null && Engine.IsEditorHint() == false)
            {
                GD.Print($"[Frame: {Engine.GetPhysicsFrames()}] [{NpcContext.Name}] [StateMachine] TRANSITION: {p_sourceState.Name} -> {nextStateName}");
            }

            if (m_currentState?.Name == nextStateName)
            {
                if (m_currentState.IsActionState)
                {
                    m_currentState.Exit();
                    m_currentState.Enter();
                }
                return;
            }

            ForceTransition(nextStateName);
        }
        else if (nextStateName != null)
        {
            if (NpcContext != null && Engine.IsEditorHint() == false)
            {
                GD.PushWarning($"[Frame: {Engine.GetPhysicsFrames()}] [{NpcContext.Name}] [StateMachine] WARNING: Attempted to transition to unknown state '{nextStateName}'");
            }
        }
    }

    private StringName GetPostGuardingState()
    {
        if (NpcContext is not IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpcGuard)
            return StateConstants.IdleStateName;

        var target = aggNpcGuard.GetTarget();
        if (target == null)
            return StateConstants.IdleStateName;

        return GetCombatDecisionState();
    }

    private StringName GetCombatDecisionState()
    {
        if (NpcContext == null)
            return StateConstants.IdleStateName;

        var target = (NpcContext as IslandSurvivor.Scenes.NPC.AggressiveNpcBase)?.GetTarget();
        return ((IslandSurvivor.Scenes.NPC.NpcBase)NpcContext).GetDecisionState(target!);
    }

    public bool HasState(Godot.StringName stateName) => m_states.ContainsKey(stateName);

    public State GetState(Godot.StringName stateName) => m_states.TryGetValue(stateName, out var state) ? state : null!;

    public void ForceTransition(StringName p_targetStateName)
    {
        if (!m_states.ContainsKey(p_targetStateName)) return;

        if (m_currentState?.Name == p_targetStateName)
        {
            if (m_currentState.IsActionState)
            {
                m_currentState.Exit();
                m_currentState.Enter();
            }
            return;
        }

        m_currentState?.Exit();
        m_currentState = m_states[p_targetStateName];
        m_currentState.Enter();
    }
}
