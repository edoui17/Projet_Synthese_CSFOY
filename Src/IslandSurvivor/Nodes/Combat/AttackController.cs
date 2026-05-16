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

    [Signal]
    public delegate void AttackActionTriggeredEventHandler();

    [Export] public StatManager? Stats { get; set; }
    [Export] public EntityFaction Faction { get; set; } = EntityFaction.None;

    [Export] public float BaseAttackCooldown { get; set; } = 1.0f;

    private AnimatedSprite2D? m_attackSprite;
    [Export]
    public AnimatedSprite2D? AttackSprite
    {
        get => m_attackSprite;
        set
        {
            if (m_attackSprite != null)
            {
                if (m_attackSprite.IsConnected(AnimatedSprite2D.SignalName.FrameChanged, new Callable(this, MethodName.OnFrameChanged)))
                {
                    m_attackSprite.Disconnect(AnimatedSprite2D.SignalName.FrameChanged, new Callable(this, MethodName.OnFrameChanged));
                }
                if (m_attackSprite.IsConnected(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, MethodName.OnAnimationFinished)))
                {
                    m_attackSprite.Disconnect(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, MethodName.OnAnimationFinished));
                }
            }

            m_attackSprite = value;

            if (m_attackSprite != null)
            {
                m_attackSprite.Connect(AnimatedSprite2D.SignalName.FrameChanged, new Callable(this, MethodName.OnFrameChanged));
                m_attackSprite.Connect(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, MethodName.OnAnimationFinished));
            }
        }
    }

    [Export] public int ActionFrame { get; set; } = 2;

    private float m_cooldownTimer = 0f;
    private bool m_hasTriggeredAction = false;

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

    private void OnFrameChanged()
    {
        if (AttackSprite == null || !IsAttacking) return;

        if (AttackSprite.Animation.ToString().Equals("Attack", System.StringComparison.OrdinalIgnoreCase) && AttackSprite.Frame >= ActionFrame && !m_hasTriggeredAction)
        {
            ExecuteAttackHit();
            EmitSignal(SignalName.AttackActionTriggered);
            m_hasTriggeredAction = true;
        }
    }

    private void OnAnimationFinished()
    {
        if (AttackSprite == null || !IsAttacking) return;

        if (AttackSprite.Animation.ToString().Equals("Attack", System.StringComparison.OrdinalIgnoreCase))
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
        m_hasTriggeredAction = false;
        m_hitTargetsThisAttack.Clear();

        float speedStat = Stats?.GetCurrentValue(StatType.Speed) ?? 0f;

        // Ensure cooldown scales with speed, but attacks are exclusively animation-driven
        float cooldown = CombatMath.CalculateTime(BaseAttackCooldown, speedStat, 0.2f);
        m_cooldownTimer = cooldown;

        m_currentActiveArea = area;

        EmitSignal(SignalName.AttackStarted);
        return true;
    }

    public void ExecuteAttackHit()
    {
        if (!IsAttacking) return;

        if (m_currentActiveArea != null)
        {
            m_currentActiveArea.Monitoring = true;
        }
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

        if (Faction == EntityFaction.Player)
        {
            if (p_node is IDamageable || p_node is IAttackable)
            {
                isValidTarget = true;
            }
        }
        else if (Faction == EntityFaction.Enemy)
        {
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
