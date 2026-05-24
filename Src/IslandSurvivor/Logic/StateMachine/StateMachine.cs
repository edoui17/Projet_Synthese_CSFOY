namespace IslandSurvivor.Logic.StateMachine;

using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class StateMachine : State
{
    [Export] public State InitialState { get; set; } = null!;

    [ExportGroup("Melee Configuration")]
    [Export] public float AttackRange { get; set; } = 60.0f;
    [Export] public float GuardChance { get; set; } = 0.3f;

    [ExportGroup("Ranged Configuration")]
    [Export] public float MinAttackRange { get; set; } = 80.0f;
    [Export] public float MaxAttackRange { get; set; } = 350.0f;

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
                m_states[state.Name] = state;
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

        StringName nextStateName = null;

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
                    nextStateName = StateConstants.ChaseStateName;
                }
                else if (p_reason == StateExitReason.CooldownFinished)
                {
                    nextStateName = GetCombatDecisionState();
                }
                else if (p_reason == StateExitReason.Finished)
                {
                    nextStateName = GetCombatDecisionState();
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
                break;

            case StateConstants.AttackState:
                if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
                {
                    var target = aggNpc.GetTarget();
                    if (target != null)
                    {
                        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.Normal.Lancer.Lancer)
                        {
                            nextStateName = new StringName("LancerRepositionState");
                        }
                        else
                        {
                            nextStateName = GetCombatDecisionState();
                        }
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
                    nextStateName = StateConstants.DashStateName; // Or "AttackState", depending on the NPC. Lancer uses DashState after WindUp. Wait, this needs to be specific.
                    // For the Lancer, WindUp goes to DashState.
                    // Let's assume it goes to AttackState by default, but if it has DashState, it goes to DashState?
                    // Let me check if DashState is present.
                    if (m_states.ContainsKey(StateConstants.DashStateName))
                        nextStateName = StateConstants.DashStateName;
                    else
                        nextStateName = StateConstants.AttackStateName;
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
                    if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.Normal.Lancer.Lancer)
                    {
                        nextStateName = StateConstants.DashStateName;
                    }
                    else
                    {
                        nextStateName = StateConstants.IdleStateName;
                    }
                }
                break;

            case StateConstants.GuardState:
                nextStateName = GetPostGuardState();
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

    private StringName GetPostGuardState()
    {
        if (NpcContext is not IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpcGuard)
            return StateConstants.IdleStateName;

        var target = aggNpcGuard.GetTarget();
        if (target == null)
            return StateConstants.IdleStateName;

        return GetCombatDecisionState();
    }

    private StringName GetCombatDecisionState()
    {
        var aggressiveNpc = NpcContext as IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase;
        if (aggressiveNpc == null)
            return StateConstants.IdleStateName;

        var target = aggressiveNpc.GetTarget();
        if (target == null)
        {
            if (m_states.TryGetValue(StateConstants.IdleStateName, out State idleStateNode) && idleStateNode is IslandSurvivor.Logic.StateMachine.States.IdleState idleState)
            {
                if (idleState.IsWanderCooldownElapsed && m_states.ContainsKey(StateConstants.WanderStateName))
                {
                    return StateConstants.WanderStateName;
                }
            }
            return StateConstants.IdleStateName;
        }

        float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
        bool isRanged = NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.RangedAggressiveNpcBase;
        float checkRange = isRanged ? MaxAttackRange : AttackRange;

        if (distSquared > checkRange * checkRange)
        {
            return StateConstants.ChaseStateName;
        }

        if (isRanged)
        {
            if (distSquared < MinAttackRange * MinAttackRange)
            {
                return StateConstants.RepositionStateName;
            }
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.Normal.Lancer.Lancer)
        {
            if (distSquared <= checkRange * checkRange)
            {
                return StateConstants.AttackStateName;
            }
            return new StringName("LancerRepositionState");
        }

        var attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");

        if (attackController != null && attackController.CanAttack)
        {
            if (aggressiveNpc.CanGuard && GD.Randf() <= GuardChance)
            {
                return StateConstants.GuardStateName;
            }

            return StateConstants.AttackStateName;
        }
        else
        {
            if (aggressiveNpc.CanGuard)
            {
                return StateConstants.GuardStateName;
            }
            else
            {
                return StateConstants.IdleStateName;
            }
        }
    }

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
