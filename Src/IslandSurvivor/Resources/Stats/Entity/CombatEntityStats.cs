using Godot;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class CombatEntityStats : EntityStats
{
    [Export]
    private float m_attack = 10f;

    [Export]
    private float m_speed = 5f;

    public float Attack => m_attack;
    public float Speed => m_speed;
}
