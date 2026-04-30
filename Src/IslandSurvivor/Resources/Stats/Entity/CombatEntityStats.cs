using Godot;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class CombatEntityStats : EntityStats
{
    private float m_attack = 10f;
    private float m_speed = 5f;

    [Export]
    public float Attack
    {
        get => m_attack;
        protected set => m_attack = value;
    }

    [Export]
    public float Speed
    {
        get => m_speed;
        protected set => m_speed = value;
    }
}
