namespace IslandSurvivor.Scenes.Projectiles;

using Godot;
using Core.Interfaces.Entities;
using Core.Interfaces.Stats;
using IslandSurvivor.Extensions;

public partial class Arrow : Area2D, IProjectile
{
    [Export] public float DefaultSpeed { get; set; } = 300.0f;
    [Export] public float DefaultDamage { get; set; } = 10.0f;

    public float Speed { get; private set; }
    public float Damage { get; private set; }
    public System.Numerics.Vector2 Direction { get; private set; }

    private Godot.Vector2 m_velocity;
    private object m_shooter;
    private bool m_isFired = false;

    // Automatically delete after a certain distance or time
    private float m_lifeTimer = 5.0f;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    public void Initialize(System.Numerics.Vector2 p_startPosition, System.Numerics.Vector2 p_direction, float p_damage, object p_shooter)
    {
        GlobalPosition = new Godot.Vector2(p_startPosition.X, p_startPosition.Y);
        Direction = p_direction;
        Godot.Vector2 directionGodot = new Godot.Vector2(Direction.X, Direction.Y).Normalized();

        Speed = DefaultSpeed;
        Damage = p_damage;
        m_shooter = p_shooter;

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

    private void OnBodyEntered(Node2D p_body)
    {
        // Don't hit the shooter
        if (p_body == m_shooter as Node2D) return;

        if (p_body is IDamageable damageable)
        {
            // If the shooter is an enemy, only hit players
            if (m_shooter is Node2D shooterNode && shooterNode.IsInGroup("EnnemiesNPC") && !p_body.IsInGroup("Player"))
            {
                return;
            }

            // If the shooter is player, only hit enemies
            if (m_shooter is Node2D sNode && sNode.IsInGroup("Player") && !p_body.IsInGroup("EnnemiesNPC"))
            {
                return;
            }

            damageable.TakeDamage((int)Damage, m_shooter);
            QueueFree();
        }
        else if (!(p_body is Area2D))
        {
            QueueFree();
        }
    }
}
