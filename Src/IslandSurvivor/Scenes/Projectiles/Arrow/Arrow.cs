namespace IslandSurvivor.Scenes.Projectiles;

using Godot;
using IslandSurvivor.Logic.Entities;
using Core.Interfaces.Stats;
using IslandSurvivor.Extensions;

public partial class Arrow : Area2D, IProjectile
{
    [Export] public float DefaultSpeed { get; set; } = 300.0f;
    [Export] public float DefaultDamage { get; set; } = 10.0f;

    public float Speed { get; private set; }
    public float Damage { get; private set; }
    public Vector2 Direction { get; private set; }

    private Godot.Vector2 m_velocity;
    private object m_shooter;
    private IslandSurvivor.Enums.EntityFaction m_faction = IslandSurvivor.Enums.EntityFaction.None;
    private bool m_isFired = false;

    // Automatically delete after a certain distance or time
    private float m_lifeTimer = 5.0f;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    public void Initialize(Vector2 p_startPosition, Vector2 p_direction, float p_damage, object p_shooter)
    {
        GlobalPosition = new Godot.Vector2(p_startPosition.X, p_startPosition.Y);
        Direction = p_direction;
        Godot.Vector2 directionGodot = new Godot.Vector2(Direction.X, Direction.Y).Normalized();

        Speed = DefaultSpeed;
        Damage = p_damage;
        m_shooter = p_shooter;

        // Determine faction based on shooter
        if (m_shooter is Node shooterNode)
        {
            var controller = shooterNode.GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
            if (controller != null)
            {
                m_faction = controller.Faction;
            }
            else if (shooterNode.IsInGroup("Player"))
            {
                m_faction = IslandSurvivor.Enums.EntityFaction.Player;
            }
            else if (shooterNode.IsInGroup("EnnemiesNPC"))
            {
                m_faction = IslandSurvivor.Enums.EntityFaction.Enemy;
            }
        }

        // Set rotation to face the direction
        Rotation = directionGodot.Angle();
        m_velocity = directionGodot * Speed;
    }

    public void Fire()
    {
        m_isFired = true;
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

    private bool IsValidTarget(Node2D p_body)
    {
        if (!(p_body is IDamageable)) return false;

        if (m_faction == IslandSurvivor.Enums.EntityFaction.Player)
        {
            return true;
        }

        if (m_faction == IslandSurvivor.Enums.EntityFaction.Enemy && p_body.IsInGroup("Player"))
        {
            return true;
        }

        return false;
    }

    private void OnBodyEntered(Node2D p_body)
    {
        // Don't hit the shooter
        if (p_body == m_shooter as Node2D) return;

        if (IsValidTarget(p_body))
        {
            // Check if target is dashing and interrupt
            if (p_body is CharacterBody2D charBody)
            {
                var movementController = charBody.GetNodeOrNull<IslandSurvivor.Nodes.Movement.MovementController>("MovementController");
                if (movementController != null && movementController.IsDashing)
                {
                    movementController.CancelDash();
                    movementController.ApplyStun(0.5f); // Half a second stun
                }
            }

            if (p_body is IDamageable damageable)
            {
                damageable.TakeDamage((int)Damage, m_shooter);
            }
            QueueFree();
            return;
        }

        bool isSolid = !(p_body is Area2D);
        if (isSolid)
        {
            // Hit a solid object we don't damage
            QueueFree();
        }
    }
}
