namespace IslandSurvivor.Scenes.NPC.Passive;

using Core.Domain;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Globals;
using IslandSurvivor.Nodes;
using System;

public partial class Sheep : PassiveNpcBase
{
    public override void _Ready()
    {
        base._Ready();
        IdleSpeed = 30.0f;
    }

    protected override void OnDamageTaken(Node2D p_attacker)
    {
        base.OnDamageTaken(p_attacker);

        AudioStream hurtStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/animal_hurt.wav");
        if (hurtStream != null)
        {
            AudioManager.Instance?.PlaySound2D(hurtStream, GlobalPosition);
        }
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
            Random random = new();
            int meatAmount = random.Next(1, 4);

            float luck = 0f;
            if (p_attacker is Node godotAttacker)
            {
                StatManager attackerStats = godotAttacker.GetNodeOrNull<StatManager>("StatManager");
                if (attackerStats != null)
                {
                    luck = attackerStats.GetCurrentValue(StatType.Luck);
                }
            }

            float bonusChance = luck * 0.05f;
            int bonusQuantity = (int)bonusChance;
            float fractionalChance = bonusChance - bonusQuantity;

            if (random.NextDouble() < fractionalChance)
            {
                bonusQuantity++;
            }

            meatAmount += bonusQuantity;

            ResourceItem meatResource = new ResourceItem("meat_01", "Viande", "Meat", "res://Assets/TinySwords/Terrain/Meat Resource/Meat Resource.png");

            Node2D? targetNode = p_attacker as Node2D;
            Vector2 fallbackPosition = targetNode != null ? targetNode.GlobalPosition : GlobalPosition;

            PackedScene dropScene = GD.Load<PackedScene>("res://Scenes/Ressources/ResourceDrop.tscn");
            if (dropScene != null)
            {
                for (int i = 0; i < meatAmount; i++)
                {
                    if (dropScene.Instantiate() is IslandSurvivor.Scenes.Ressources.ResourceDrop drop)
                    {
                        drop.Initialize(meatResource, 1, GlobalPosition, targetNode, fallbackPosition);
                        GetParent().AddChild(drop);
                    }
                }
            }
            else
            {
                if (SignalManager.Instance != null)
                {
                    SignalManager.Instance.EmitMaterialDestroyed(this, meatResource, meatAmount);
                    GD.Print($"Sheep died. Sent {meatAmount} meat to inventory via SignalManager.");
                }
                else
                {
                    GD.PrintErr("SignalManager is not available.");
                }
            }
        }

        AudioStream deathStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/animal_death.wav");
        if (deathStream != null)
        {
            AudioManager.Instance?.PlaySound2D(deathStream, GlobalPosition);
        }

        QueueFree();
    }
}