using Godot;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class EntityStats : Resource
{
    [Export]
    private float m_maxHealth = 100f;

    public float MaxHealth => m_maxHealth;
}
