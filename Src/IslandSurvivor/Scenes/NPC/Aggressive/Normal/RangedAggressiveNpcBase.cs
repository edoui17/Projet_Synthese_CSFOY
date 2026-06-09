namespace IslandSurvivor.Scenes.NPC;

using Godot;
using IslandSurvivor.Logic;
using IslandSurvivor.Globals;

public partial class RangedAggressiveNpcBase : AggressiveNpcBase
{


    public override void _Ready()
    {
        base._Ready();

        if (m_attackController != null)
        {
            m_attackController.Stats = Stats;
            m_attackController.Faction = IslandSurvivor.Enums.EntityFaction.Enemy;

            m_attackController.AttackStarted += OnAttackStarted;

        }
        else
        {
            GD.PushWarning($"{Name}: AttackController not found.");
        }
    }

    protected virtual void OnAttackStarted()
    {
    }


}
