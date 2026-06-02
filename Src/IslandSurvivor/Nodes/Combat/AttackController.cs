namespace IslandSurvivor.Nodes;

using Godot;
using IslandSurvivor.Enums;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Logic;
using Core.Interfaces;
using Core.Managers;
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
        // 1. Force a clean state before starting
        if (IsAttacking)
        {
            GD.PushWarning("TryAttack called while already attacking. Forcing cleanup.");
            EndAttack();
        }

        if (!CanAttack) return false;

        IsAttacking = true;
        m_hitTargetsThisAttack.Clear();

        // 2. Try getting a registered hitbox. If none exist (e.g., Archer), that's fine.
        if (m_directionAreas.TryGetValue(p_direction, out Area2D? area) && GodotObject.IsInstanceValid(area))
        {
            m_currentActiveArea = area;
        }
        else
        {
            m_currentActiveArea = null;
        }

        // ... proceed with animation
        if (m_animationPlayer != null && !string.IsNullOrEmpty(AttackAnimationName))
        {
            m_animationPlayer.Play(AttackAnimationName);
        }
        return true;
    }

    // Public method intended to be called exclusively by the Godot AnimationPlayer via a Method Track
    public void ExecuteAttackHit()
    {
        // Ensure we are still in a valid state
        if (!IsAttacking) return;

        EmitSignal(SignalName.AttackActionTriggered);

        // Ranged units or units without specific hitboxes will have m_currentActiveArea as null
        if (!GodotObject.IsInstanceValid(m_currentActiveArea)) return;

        m_currentActiveArea.Monitoring = true;

        // Use CallDeferred to ensure we aren't modifying physics state
        // mid-frame during an animation playback
        Callable.From(() =>
        {
            if (!GodotObject.IsInstanceValid(m_currentActiveArea)) return;

            var overlappingBodies = m_currentActiveArea.GetOverlappingBodies();
            foreach (var body in overlappingBodies) ProcessHit(body);

            var overlappingAreas = m_currentActiveArea.GetOverlappingAreas();
            foreach (var area in overlappingAreas)
            {
                ProcessHit(area);
                ProcessHit(area.GetParent());
            }
        }).CallDeferred();
    }

    public void CancelAttack()
    {
        // The previous EndAttack() method might be too aggressive.
        // Let's make it null-safe and check if we are already finished.
        if (!IsAttacking) return;

        EndAttack();
    }

    public void ResetCooldown()
    {
        m_cooldownTimer = 0f;
    }

    private void EndAttack()
    {
        IsAttacking = false;

        // Safety check: ensure we don't access a freed object
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
