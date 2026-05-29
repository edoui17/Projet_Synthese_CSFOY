namespace IslandSurvivor.Logic;

using Godot;
using IslandSurvivor.Logic;
using Core.Interfaces;
using IslandSurvivor.Extensions;
using IslandSurvivor.Enums;

[GlobalClass]
public partial class BaseProjectile : Area2D, IProjectile
{
    [Export] public float DefaultSpeed { get; set; } = 300.0f;
    [Export] public float DefaultDamage { get; set; } = 10.0f;
    [Export] public string AttackAnimation { get; set; } = "Attack";
    [Export] public float LifeTime { get; set; } = 5.0f;
    [Export] public bool DestroyOnImpact { get; set; } = true;

    private const string GROUP_PLAYER = "Player";
    private const string GROUP_ENEMIES_NPC = "EnnemiesNPC";
    private const string GROUP_ENEMY = "Ennemy";
    private const string NODE_ATTACK_CONTROLLER = "AttackController";
    private const string NODE_MOVEMENT_CONTROLLER = "MovementController";

    public float Speed { get; protected set; }
    public float Damage { get; protected set; }
    public Vector2 Direction { get; protected set; }

    protected Godot.Vector2 m_velocity;
    protected object m_shooter = null!;
    protected EntityFaction m_faction = EntityFaction.None;
    protected bool m_isFired = false;
    protected float m_lifeTimer;

    protected AnimatedSprite2D m_animatedSprite = null!;

    public override void _Ready()
    {
        base._Ready();

        m_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;
    }

    public virtual void Initialize(Vector2 p_startPosition, Vector2 p_direction, float p_damage, object p_shooter)
    {
        GlobalPosition = new Godot.Vector2(p_startPosition.X, p_startPosition.Y);
        Direction = p_direction;
        Godot.Vector2 directionGodot = new Godot.Vector2(Direction.X, Direction.Y).Normalized();

        Speed = DefaultSpeed;
        Damage = p_damage;
        m_shooter = p_shooter;
        m_lifeTimer = LifeTime;

        // Determine faction based on shooter
        if (m_shooter is Node shooterNode)
        {
            var controller = shooterNode.GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>(NODE_ATTACK_CONTROLLER);
            if (controller != null)
            {
                m_faction = controller.Faction;
            }
            else if (shooterNode.IsInGroup(GROUP_PLAYER))
            {
                m_faction = EntityFaction.Player;
            }
            else if (shooterNode.IsInGroup(GROUP_ENEMIES_NPC))
            {
                m_faction = EntityFaction.Enemy;
            }
        }

        // Set rotation to face the direction
        Rotation = directionGodot.Angle();
        m_velocity = directionGodot * Speed;
    }

    public virtual void Fire()
    {
        m_isFired = true;
        if (m_animatedSprite != null && m_animatedSprite.SpriteFrames != null && m_animatedSprite.SpriteFrames.HasAnimation(AttackAnimation))
        {
            m_animatedSprite.Play(AttackAnimation);
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (!m_isFired) return;

        Position += m_velocity * (float)p_delta;

        m_lifeTimer -= (float)p_delta;
        if (m_lifeTimer <= 0)
        {
            QueueFree();
        }
    }

    protected virtual bool IsValidTarget(Node2D p_body)
    {
        if (!(p_body is IDamageable)) return false;

        if (m_faction == EntityFaction.Player)
        {
            return p_body.IsInGroup(GROUP_ENEMIES_NPC) || p_body.IsInGroup(GROUP_ENEMY); // Assuming generic enemy tags
        }

        if (m_faction == EntityFaction.Enemy && p_body.IsInGroup(GROUP_PLAYER))
        {
            return true;
        }

        return false;
    }

    protected virtual void OnAreaEntered(Area2D p_area)
    {
        HandleCollision(p_area);
    }

    protected virtual void OnBodyEntered(Node2D p_body)
    {
        HandleCollision(p_body);
    }

    protected virtual void HandleCollision(Node2D p_node)
    {
        // Don't hit the shooter
        if (p_node == m_shooter as Node2D) return;

        if (!IsValidTarget(p_node))
        {
            bool isSolid = !(p_node is Area2D); // Assuming non-areas (CharacterBody2D, StaticBody2D) might be solid
                                                // Refine this check if there's a specific collision layer for walls

            if (isSolid && DestroyOnImpact)
            {
                QueueFree();
            }
            return;
        }

        // Check if target is dashing and interrupt
        if (p_node is CharacterBody2D charBody)
        {
            var movementController = charBody.GetNodeOrNull<IslandSurvivor.Nodes.Movement.MovementController>(NODE_MOVEMENT_CONTROLLER);
            if (movementController != null && movementController.IsDashing)
            {
                movementController.CancelDash();
                movementController.ApplyStun(0.5f); // Half a second stun
            }
        }

        if (p_node is IDamageable damageable)
        {
            damageable.TakeDamage((int)Damage, m_shooter);
        }

        if (DestroyOnImpact)
        {
            QueueFree();
        }
    }
}