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

    [Export] public string AttackAnimationName { get; set; } = "Attack";
    private StringName m_cachedAttackAnimationName = null!;

    public void SetAttackAnimation(string p_animationName)
    {
        AttackAnimationName = p_animationName;
        m_cachedAttackAnimationName = new StringName(AttackAnimationName);
    }

    private AnimatedSprite2D? m_legacyAttackSprite;
    public AnimatedSprite2D? LegacyAttackSprite
    {
        get => m_legacyAttackSprite;
        set
        {
            if (m_legacyAttackSprite != null)
            {
                if (m_legacyAttackSprite.IsConnected(AnimatedSprite2D.SignalName.FrameChanged, new Callable(this, MethodName.OnLegacyFrameChanged)))
                {
                    m_legacyAttackSprite.Disconnect(AnimatedSprite2D.SignalName.FrameChanged, new Callable(this, MethodName.OnLegacyFrameChanged));
                }
                if (m_legacyAttackSprite.IsConnected(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, MethodName.OnLegacyAnimationFinished)))
                {
                    m_legacyAttackSprite.Disconnect(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, MethodName.OnLegacyAnimationFinished));
                }
            }
            m_legacyAttackSprite = value;
            if (m_legacyAttackSprite != null)
            {
                m_legacyAttackSprite.Connect(AnimatedSprite2D.SignalName.FrameChanged, new Callable(this, MethodName.OnLegacyFrameChanged));
                m_legacyAttackSprite.Connect(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, MethodName.OnLegacyAnimationFinished));
            }
        }
    }

    private void OnLegacyFrameChanged()
    {
        if (LegacyAttackSprite == null || !IsAttacking) return;
        if (LegacyAttackSprite.Animation == m_cachedAttackAnimationName && LegacyAttackSprite.Frame >= ActionFrame && !m_hasTriggeredAction)
        {
            ExecuteAttackHit();
            EmitSignal(SignalName.AttackActionTriggered);
            m_hasTriggeredAction = true;
        }
    }

    private void OnLegacyAnimationFinished()
    {
        if (LegacyAttackSprite == null || !IsAttacking) return;
        if (LegacyAttackSprite.Animation == m_cachedAttackAnimationName)
        {
            CancelAttack();
        }
    }

    private Sprite2D? m_attackSprite;
    public Sprite2D? AttackSprite
    {
        get => m_attackSprite;
        set
        {
            if (m_attackSprite != null)
            {
                if (m_attackSprite.IsConnected(Sprite2D.SignalName.FrameChanged, new Callable(this, MethodName.OnFrameChanged)))
                {
                    m_attackSprite.Disconnect(Sprite2D.SignalName.FrameChanged, new Callable(this, MethodName.OnFrameChanged));
                }
            }
            m_attackSprite = value;
            if (m_attackSprite != null)
            {
                m_attackSprite.Connect(Sprite2D.SignalName.FrameChanged, new Callable(this, MethodName.OnFrameChanged));
            }
        }
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

    [Export] public int ActionFrame { get; set; } = 2;

    private float m_cooldownTimer = 0f;
    private bool m_hasTriggeredAction = false;

    public bool IsAttacking { get; private set; } = false;
    public bool CanAttack => !IsAttacking && m_cooldownTimer <= 0f;

    private System.Collections.Generic.Dictionary<string, Area2D> m_directionAreas = new();
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

    private void OnFrameChanged()
    {
        if (AttackSprite == null || !IsAttacking) return;

        // With AnimationPlayer, the Sprite2D.Animation string doesn't exist. We check AnimationPlayer's current animation.
        bool isCorrectAnimation = (AttackAnimationPlayer != null && AttackAnimationPlayer.CurrentAnimation == AttackAnimationName);

        if (isCorrectAnimation && AttackSprite.Frame >= ActionFrame && !m_hasTriggeredAction)
        {
            ExecuteAttackHit();
            EmitSignal(SignalName.AttackActionTriggered);
            m_hasTriggeredAction = true;
        }
    }

    private void OnAnimationFinished(Godot.StringName p_animName)
    {
        if (!IsAttacking) return;

        if (p_animName == AttackAnimationName)
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

            // Manually check for already overlapping bodies and areas to prevent missed hits
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
