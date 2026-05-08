using Godot;

namespace IslandSurvivor.Resources;

[GlobalClass]
public partial class PlayerStats : CombatEntityStats
{
    [Export]
    private float m_luck = 1f;

    public float Luck => m_luck;
}
