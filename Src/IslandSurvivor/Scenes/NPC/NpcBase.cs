using IslandSurvivor.Nodes;
namespace IslandSurvivor.Scenes.NPC;

using Godot;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes.Movement;

public partial class NpcBase : CharacterBody2D, INpc, IDamageable
{
    [Export] public string NpcType { get; set; } = "NPC";
    [Export] public float IdleSpeed { get; set; } = 50.0f;

    [Export] public StatManager Stats { get; set; } = null!;

    [ExportGroup("Audio")]
    [Export] public AudioStream? HurtSound { get; set; }
    [Export] public string HurtSoundKey { get; set; } = string.Empty;
    [Export] public float HurtVolume { get; set; } = 1.0f;

    [Export] public AudioStream? DeathSound { get; set; }
    [Export] public string DeathSoundKey { get; set; } = string.Empty;
    [Export] public float DeathVolume { get; set; } = 1.0f;

    [Export] public float AudioMaxDistance { get; set; } = 2000f;
    [Export] public float AudioAttenuation { get; set; } = 1f;

    protected MovementController? m_movementController;
    protected IslandSurvivor.Logic.StateMachine.StateMachine? m_stateMachine;

    public MovementController? MovementController => m_movementController;

    protected object? m_lastAttacker = null;
    protected bool m_wasKilledByPlayer = false;

    public virtual string CurrentState => m_stateMachine?.CurrentState?.Name ?? "";

    public virtual Godot.StringName GetDecisionState(Node2D target)
    {
        return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
    }

    public override void _Ready()
    {
        base._Ready();

        m_movementController = GetNodeOrNull<MovementController>("MovementController");

        m_stateMachine = GetNodeOrNull<IslandSurvivor.Logic.StateMachine.StateMachine>("StateMachine");
        if (m_stateMachine != null)
        {
            m_stateMachine.Initialize(null, this);

            var deathState = m_stateMachine.GetNodeOrNull<IslandSurvivor.Logic.StateMachine.States.DeathState>("DeathState");
            if (deathState != null)
            {
                deathState.StateFinished += OnDeathStateFinished;
            }
        }


        if (Stats != null)
        {
            Stats.Connect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
        else
        {
            GD.PushWarning($"{Name} node is missing a StatManager node reference.");
        }
    }

    public override void _Process(double p_delta)
    {
        m_stateMachine?.Update(p_delta);
    }

    public override void _PhysicsProcess(double p_delta)
    {
        m_stateMachine?.PhysicsUpdate(p_delta);
    }

    protected virtual void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        if ((StatType)p_statType == StatType.Health && p_currentValue <= 0)
        {
            if (Stats != null)
            {
                Stats.Disconnect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
            }
            HandleDeath(m_lastAttacker);
        }
    }

    public virtual void TakeDamage(int p_amount, object p_attacker)
    {
        m_lastAttacker = p_attacker;

        if (Stats != null)
        {
            float currentHp = Stats.GetCurrentValue(StatType.Health);
            Stats.SetCurrentValue(StatType.Health, currentHp - p_amount);
        }

        bool isDead = Stats == null || Stats.GetCurrentValue(StatType.Health) <= 0;

        if (p_attacker is Node2D attackerNode)
        {
            if (isDead && attackerNode.IsInGroup("Player"))
            {
                m_wasKilledByPlayer = true;
            }

            if (!isDead)
            {
                OnDamageTaken(attackerNode);
                IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);
                IslandSurvivor.Extensions.NodeExtensions.PlayShake(this);
            }
        }
    }

    protected virtual void OnDamageTaken(Node2D p_attacker)
    {
    }

    protected virtual void HandleDeath(object? p_attacker = null) { }

    protected virtual void OnDeathStateFinished(IslandSurvivor.Logic.StateMachine.State p_sourceState, IslandSurvivor.Logic.StateMachine.StateExitReason p_reason)
    {
        if (p_reason == IslandSurvivor.Logic.StateMachine.StateExitReason.Finished)
        {
            QueueFree();
        }
    }
}