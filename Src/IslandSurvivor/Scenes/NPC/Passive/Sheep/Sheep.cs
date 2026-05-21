using Godot;
using System;
using Core.Domain;
using IslandSurvivor.Logic.Entities;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes.Movement;
using Core.Interfaces;
using Core.Interfaces.Stats;
using IslandSurvivor.Nodes;
using Core.Managers.Stats;
using IslandSurvivor.Scenes.NPC.Passive;
using IslandSurvivor.Globals;

public partial class Sheep : PassiveNpcBase
{
    public override void _Ready()
    {
        base._Ready();
        IdleSpeed = 30.0f;
    }

    protected override void HandleDeath(object? p_attacker = null)
    {
        m_passiveController.SetDead();

        if (ServiceRegistry.Instance != null)
        {
            ServiceRegistry.Instance.EventBus.Publish(new Core.Events.EnemyKilledEvent(Name, NpcType, 0f));
        }

        if (m_wasKilledByPlayer)
        {
            int baseMeatAmount = (int)(GD.Randi() % 3) + 1; // 1 to 3 meat
            int meatAmount = IslandSurvivor.Logic.ResourceUtils.CalculateYield(baseMeatAmount, p_attacker);

            ResourceItem meatResource = new ResourceItem("meat_01", "Viande", "Meat", "res://Assets/TinySwords/TinySwords(Update010)/Deco/17.png");

            if (SignalManager.Instance != null)
            {
                SignalManager.Instance.EmitMaterialDestroyed(this, meatResource, meatAmount);
                GD.Print($"Sheep died. Sent {meatAmount} meat to inventory.");
            }
            else
            {
                GD.PrintErr("SignalManager is not available.");
            }
        }

        QueueFree();
    }
}
