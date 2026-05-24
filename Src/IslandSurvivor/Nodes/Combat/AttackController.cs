namespace IslandSurvivor.Nodes.Combat;

using Godot;
using IslandSurvivor.Enums;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Logic;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using System.Collections.Generic;

[GlobalClass]
public partial class AttackController : Node
{
    [Signal] public delegate void AttackStartedEventHandler();
    [Signal] public delegate void AttackFinishedEventHandler();
    [Signal] public delegate void AttackActionTriggeredEventHandler();
    [Signal] public delegate void TargetHitEventHandler(Node p_target, int p_damage);

    [Export] public float BaseAttackCooldown { get; set; } = 1.0f;
    [Export] public EntityFaction Faction { get; set; } = EntityFaction.Player;
    [Export] public string AttackAnimationName { get; set; } = "Attack";

    public StatManager? Stats { get; set; }

    private StringName m_cachedAttackAnimationName = new StringName();

    public void SetAttackAnimation(string p_animName)
    {
        AttackAnimationName = p_animName;
        m_cachedAttackAnimationName = new StringName(p_animName);
    }

    // Unused, keeping for API compatibility if something binds to it, but it no longer drives frame events.
    private Sprite2D? m_attackSprite;
    public Sprite2D? AttackSprite
    {
        get => m_attackSprite;
        set => m_attackSprite = value;
    }

    private AnimationPlayer? m_animationPlayer;
    public AnimationPlayer? AttackAnimationPlayer
    {
        get => m_animationPlayer;
        set
        {
            if (m_animationPlayer != null)
            {
                if (m_animationPlayer.IsConnected(AnimationPlayer.SignalName.AnimationFinished, new Callable(this, MethodName.OnAnimationFinished)))
                {
                    m_animationPlayer.Disconnect(AnimationPlayer.SignalName.AnimationFinished, new Callable(this, MethodName.OnAnimationFinished));
                }
            }
            m_animationPlayer = value;
            if (m_animationPlayer != null)
            {
                m_animationPlayer.Connect(AnimationPlayer.SignalName.AnimationFinished, new Callable(this, MethodName.OnAnimationFinished));
            }
        }
    }

    private float m_cooldownTimer = 0f;

    public bool IsAttacking { get; private set; } = false;
    public bool CanAttack => !IsAttacking && m_cooldownTimer <= 0f;

    private Dictionary<string, Area2D> m_directionAreas = new();
    private HashSet<object> m_hitTargetsThisAttack = new();
    private Area2D? m_currentActiveArea;
    private Node? m_owner;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) return;

        m_cachedAttackAnimationName = new StringName(AttackAnimationName);

        m_owner = GetOwner<Node>();
        if (m_owner == null)
        {
            m_owner = GetParent();
        }
    }

    private void OnAnimationFinished(Godot.StringName p_animName)
    {
        if (!IsAttacking) return;

        if (p_animName == m_cachedAttackAnimationName)
        {
            CancelAttack();
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
    }

    public void RegisterArea(string p_direction, Area2D p_area)
    {
        m_directionAreas[p_direction] = p_area;
        p_area.Monitoring = false;

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

        IsAttacking = true;
        m_hitTargetsThisAttack.Clear();

        float speedStat = Stats?.GetCurrentValue(StatType.Speed) ?? 0f;

        float cooldown = CombatMath.CalculateTime(BaseAttackCooldown, speedStat, 0.2f);
        m_cooldownTimer = cooldown;

        m_currentActiveArea = area;

        if (m_animationPlayer != null && !string.IsNullOrEmpty(AttackAnimationName))
        {
            m_animationPlayer.Play(AttackAnimationName);
        }

        EmitSignal(SignalName.AttackStarted);
        return true;
    }

    // Public method intended to be called exclusively by the Godot AnimationPlayer via a Method Track
    public void ExecuteAttackHit()
    {
        if (!IsAttacking) return;

        EmitSignal(SignalName.AttackActionTriggered);

        if (GodotObject.IsInstanceValid(m_currentActiveArea))
        {
            m_currentActiveArea.Monitoring = true;

            var overlappingBodies = m_currentActiveArea.GetOverlappingBodies();
            foreach (var body in overlappingBodies)
            {
                ProcessHit(body);
            }

            var overlappingAreas = m_currentActiveArea.GetOverlappingAreas();
            foreach (var area in overlappingAreas)
            {
                ProcessHit(area);
                ProcessHit(area.GetParent());
            }
        }
    }

    public void CancelAttack()
    {
        if (IsAttacking)
        {
            EndAttack();
        }
    }

    public void ResetCooldown()
    {
        m_cooldownTimer = 0f;
    }

    private void EndAttack()
    {
        IsAttacking = false;
        if (GodotObject.IsInstanceValid(m_currentActiveArea))
        {
            m_currentActiveArea.Monitoring = false;
        }
        m_currentActiveArea = null;
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

    private bool IsValidTarget(Node p_node)
    {
        if (Faction == EntityFaction.Player)
        {
            return p_node is IDamageable || p_node is IAttackable;
        }

        if (Faction == EntityFaction.Enemy)
        {
            return p_node is IDamageable && p_node.IsInGroup("Player");
        }

        return false;
    }

    private void ProcessHit(Node? p_node)
    {
        if (p_node == null || p_node == m_owner) return;
        if (!IsAttacking) return;
        if (m_hitTargetsThisAttack.Contains(p_node)) return;

        if (IsValidTarget(p_node))
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
        else if (p_target is IAttackable)
        {
            if (p_target is IDamageable resourceDamageable)
            {
                resourceDamageable.TakeDamage(finalDamage, m_owner ?? this);
                EmitSignal(SignalName.TargetHit, p_target, finalDamage);
            }
        }
    }
}
