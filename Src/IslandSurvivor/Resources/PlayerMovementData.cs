using Godot;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class PlayerMovementData : Resource
{
    private float m_acceleration = 2000f;
    private float m_friction = 1500f;

    [Export]
    public float Acceleration
    {
        get => m_acceleration;
        private set => m_acceleration = value;
    }

    [Export]
    public float Friction
    {
        get => m_friction;
        private set => m_friction = value;
    }
}
