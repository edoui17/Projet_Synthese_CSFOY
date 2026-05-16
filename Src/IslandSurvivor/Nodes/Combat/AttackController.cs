using Godot;
using System.Collections.Generic;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using IslandSurvivor.Enums;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Logic;

namespace IslandSurvivor.Nodes.Combat;

[GlobalClass]
public partial class AttackController : Node
{
    [Signal]
    public delegate void AttackStartedEventHandler();

    [Signal]
    public delegate void TargetHitEventHandler(Node p_target, int p_damageDealt);

    [Signal]
    public delegate void AttackFinishedEventHandler();

    [Export] public StatManager? Stats { get; set; }
    [Export] public EntityFaction Faction { get; set; } = EntityFaction.None;

    [Export] public float BaseAttackCooldown { get; set; } = 1.0f;
    [Export] public float BaseAttackDuration { get; set; } = 0.4f;

    private float m_cooldownTimer = 0f;
    private float m_durationTimer = 0f;
    public bool IsAttacking { get; private set; } = false;
    public bool CanAttack => !IsAttacking && m_cooldownTimer <= 0f;

    private Godot.Collections.Dictionary<string, Area2D> m_directionAreas = new();
    private HashSet<object> m_hitTargetsThisAttack = new();
    private Area2D? m_currentActiveArea;
    private Node? m_owner;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) return;
        m_owner = GetOwner<Node>();
        if (m_owner == null)
        {
            m_owner = GetParent();
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (Engine.IsEditorHint()) return;

        float delta = (float)p_delta;

        if (m_cooldownTimer > 0f)
        {
            m_cooldownTimer -= delta;
        }

        if (IsAttacking)
        {
            m_durationTimer -= delta;
            if (m_durationTimer <= 0f)
            {
                EndAttack();
            }
        }
    }

    public void RegisterArea(string p_direction, Area2D p_area)
    {
        m_directionAreas[p_direction] = p_area;
        p_area.Monitoring = false;

        // Remove existing connections to prevent duplicates
        if (p_area.IsConnected(Area2D.SignalName.AreaEntered, new Callable(this, MethodName.OnAreaEntered)))
        {
            p_area.Disconnect(Area2D.SignalName.AreaEntered, new Callable(this, MethodName.OnAreaEntered));
        }
        if (p_area.IsConnected(Area2D.SignalName.BodyEntered, new Callable(this, MethodName.OnBodyEntered)))
        {
            p_area.Disconnect(Area2D.SignalName.BodyEntered, new Callable(this, MethodName.OnBodyEntered));
        }

        p_area.Connect(Area2D.SignalName.AreaEntered, new Callable(this, MethodName.OnAreaEntered));
        p_area.Connect(Area2D.SignalName.BodyEntered, new Callable(this, MethodName.OnBodyEntered));
    }

    public bool TryAttack(string p_direction)
    {
        if (!CanAttack) return false;

        Area2D? area = null;
        m_directionAreas.TryGetValue(p_direction, out area);

        // For ranged enemies, they might not have areas registered, so we allow it to proceed without one.

        IsAttacking = true;
        m_hitTargetsThisAttack.Clear();

        float speedStat = Stats?.GetCurrentValue(StatType.Speed) ?? 0f;

        // Attack duration is scaled by speed
        float duration = CombatMath.CalculateTime(BaseAttackDuration, speedStat, 0.1f);
        m_durationTimer = duration;

        // Cooldown starts scaling by speed
        float cooldown = CombatMath.CalculateTime(BaseAttackCooldown, speedStat, 0.2f);
        // Ensure cooldown is at least as long as duration
        m_cooldownTimer = Mathf.Max(cooldown, duration);

        m_currentActiveArea = area;
        if (m_currentActiveArea != null)
        {
            m_currentActiveArea.Monitoring = true;
        }

        EmitSignal(SignalName.AttackStarted);
        return true;
    }

    public void CancelAttack()
    {
        if (IsAttacking)
        {
            EndAttack();
        }
    }

    private void EndAttack()
    {
        IsAttacking = false;
        if (m_currentActiveArea != null)
        {
            m_currentActiveArea.Monitoring = false;
            m_currentActiveArea = null;
        }
        m_hitTargetsThisAttack.Clear();
        EmitSignal(SignalName.AttackFinished);
    }

    private void OnAreaEntered(Area2D p_area)
    {
        ProcessHit(p_area);
        ProcessHit(p_area.GetParent());
    }

    private void OnBodyEntered(Node2D p_body)
    {
        ProcessHit(p_body);
    }

    private void ProcessHit(Node? p_node)
    {
        if (p_node == null || p_node == m_owner) return;
        if (!IsAttacking) return;
        if (m_hitTargetsThisAttack.Contains(p_node)) return;

        bool isValidTarget = false;

        // Faction check
        if (Faction == EntityFaction.Player)
        {
            // Player damages enemies and resources
            if (p_node is IDamageable || p_node is IAttackable)
            {
                isValidTarget = true;
            }
        }
        else if (Faction == EntityFaction.Enemy)
        {
            // Enemy damages only players. In our game, Player is the only other entity with IDamageable that isn't Enemy/Resource,
            // but we can be more strict if we assume Player is in group "Player" or just by duck typing.
            // For now, Player implements IDamageable and is not an IAttackable resource.
            if (p_node is IDamageable && p_node.IsInGroup("Player"))
            {
                isValidTarget = true;
            }
        }

        if (isValidTarget)
        {
            ApplyDamage(p_node);
        }
    }

    private void ApplyDamage(Node p_target)
    {
        m_hitTargetsThisAttack.Add(p_target);

        float attackStat = Stats?.GetCurrentValue(StatType.Attack) ?? 0f;
        float baseDamage = Stats?.BaseAttackValue ?? 10f;
        int finalDamage = CombatMath.CalculateDamage(baseDamage, attackStat);

        if (p_target is IDamageable damageable)
        {
            damageable.TakeDamage(finalDamage, m_owner ?? this);
            EmitSignal(SignalName.TargetHit, p_target, finalDamage);
        }
        else if (p_target is IAttackable attackable)
        {
            // For resources
            // Note: Currently resources handle their own specific interactions, but we might want them to take damage uniformly
            // In existing logic, the player uses Interaction, not attacks, for resources.
            // If we are extending Attack to resources, we might need a method on IAttackable.
            // We'll leave it simple for now or call TakeDamage if they implement it.
            if (p_target is IDamageable resourceDamageable)
            {
                resourceDamageable.TakeDamage(finalDamage, m_owner ?? this);
                EmitSignal(SignalName.TargetHit, p_target, finalDamage);
            }
        }
    }
}
