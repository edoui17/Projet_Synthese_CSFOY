using Godot;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class EntityStats : Resource
{
    private float m_maxHealth = 100f;

    [Export]
    public float MaxHealth
    {
        get => m_maxHealth;
        protected set => m_maxHealth = value;
    }
}
