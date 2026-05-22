namespace IslandSurvivor.Scenes.Projectiles;

using Godot;
using IslandSurvivor.Logic.Projectiles;

public partial class DarkScythe : BaseProjectile
{
    private bool m_isReturning = false;
    private float m_flightTime = 0.0f;

    [Export] public float ReturnTime { get; set; } = 1.5f;
    [Export] public float SpinSpeed { get; set; } = 15.0f;
    [Export] public float MaxScaleMultiplier { get; set; } = 5.0f;

    private Vector2 m_baseScale;
    private Vector2 m_originalPosition;
    private CollisionShape2D m_collisionShape;
    private Vector2 m_baseCollisionScale;

    public override void _Ready()
    {
        base._Ready();
        m_baseScale = Scale;
        m_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (m_collisionShape != null)
        {
            m_baseCollisionScale = m_collisionShape.Scale;
        }
    }

    public override void Initialize(Vector2 p_startPosition, Vector2 p_direction, float p_damage, object p_shooter)
    {
        base.Initialize(p_startPosition, p_direction, p_damage, p_shooter);
        m_originalPosition = new Vector2(p_startPosition.X, p_startPosition.Y);

        // Ensure scythe lives long enough to return. Timeout as safety
        if (LifeTime < ReturnTime * 2.5f)
        {
            LifeTime = ReturnTime * 2.5f;
            m_lifeTimer = LifeTime;
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (!m_isFired) return;

        m_flightTime += (float)p_delta;

        // Spin the scythe
        Rotation += SpinSpeed * (float)p_delta;

        // Note: The scale multiplier is determined by flight time / return time.
        // 0.0 means start, 1.0 means max size.
        float scaleProgress = Mathf.Clamp(m_flightTime / ReturnTime, 0.0f, 1.0f);
        if (m_isReturning)
        {
            scaleProgress = 1.0f - scaleProgress;
        }

        float currentMultiplier = Mathf.Lerp(1.0f, MaxScaleMultiplier, scaleProgress);

        // We scale ONLY the CollisionShape2D, leaving the root node scale intact
        if (m_collisionShape != null)
        {
            m_collisionShape.Scale = m_baseCollisionScale * currentMultiplier;
        }

        if (m_flightTime >= ReturnTime && !m_isReturning)
        {
            m_isReturning = true;
            m_flightTime = 0.0f; // Reset flight time to easily calculate scaling on return
        }

        if (m_isReturning && m_shooter is Node2D returnShooterNode && IsInstanceValid(returnShooterNode))
        {
            // Re-calculate velocity to track the shooter dynamically on the way back
            Godot.Vector2 directionToShooter = (returnShooterNode.GlobalPosition - GlobalPosition).Normalized();
            m_velocity = directionToShooter * Speed;
        }

        Position += m_velocity * (float)p_delta;

        if (m_isReturning)
        {
            // Use the shooter's position to check for returning, or original position if shooter died
            Godot.Vector2 targetPos = new Godot.Vector2(m_originalPosition.X, m_originalPosition.Y);
            if (m_shooter is Node2D targetShooterNode && IsInstanceValid(targetShooterNode))
            {
                targetPos = targetShooterNode.GlobalPosition;
            }

            // Destroy if close enough to target position
            // Using DistanceSquaredTo to avoid square root
            float distSq = GlobalPosition.DistanceSquaredTo(targetPos);
            if (distSq < 40.0f * 40.0f) // Slightly larger detection radius for catching it
            {
                QueueFree();
                return;
            }
        }

        m_lifeTimer -= (float)p_delta;
        if (m_lifeTimer <= 0)
        {
            QueueFree();
        }
    }
}