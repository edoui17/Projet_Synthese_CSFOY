using Godot;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class EntityStats : Resource
{
    private float m_maxHealth = 100f;
    private float m_attack = 10f;
    private float m_speed = 5f;
    private float m_luck = 1f;

    [Export]
    public float MaxHealth
    {
        get => m_maxHealth;
        private set => m_maxHealth = value;
    }

    [Export]
    public float Attack
    {
        get => m_attack;
        private set => m_attack = value;
    }

    [Export]
    public float Speed
    {
        get => m_speed;
        private set => m_speed = value;
    }

    [Export]
    public float Luck
    {
        get => m_luck;
        private set => m_luck = value;
    }
}
