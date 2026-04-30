using Godot;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class PlayerStats : CombatEntityStats
{
    private float m_luck = 1f;

    [Export]
    public float Luck
    {
        get => m_luck;
        protected set => m_luck = value;
    }
}
