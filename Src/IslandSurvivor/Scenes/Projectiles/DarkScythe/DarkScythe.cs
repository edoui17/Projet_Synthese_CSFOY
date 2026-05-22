namespace IslandSurvivor.Scenes.Projectiles;

using Godot;
using IslandSurvivor.Logic.Projectiles;

public partial class DarkScythe : BaseProjectile
{
    private bool m_isReturning = false;
    private float m_flightTime = 0.0f;
    [Export] public float ReturnTime { get; set; } = 1.5f;

    public override void _PhysicsProcess(double p_delta)
    {
        if (!m_isFired) return;

        m_flightTime += (float)p_delta;

        if (m_flightTime >= ReturnTime && !m_isReturning)
        {
            m_isReturning = true;
            m_velocity = -m_velocity; // Reverse direction
        }

        Position += m_velocity * (float)p_delta;

        m_lifeTimer -= (float)p_delta;
        if (m_lifeTimer <= 0)
        {
            QueueFree();
        }
    }
}