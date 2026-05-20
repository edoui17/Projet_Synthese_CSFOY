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
    [ExportGroup("Audio")]
    [Export] public AudioStream? HurtSound { get; set; }
    [Export] public AudioStream? DeathSound { get; set; }
    [Export] public AudioStream? IdleSound { get; set; }

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
            Random random = new();
            int meatAmount = random.Next(1, 4); // 1 to 3 meat

            float luck = 0f;
            if (p_attacker is Node GodotAttacker)
            {
                StatManager attackerStats = GodotAttacker.GetNodeOrNull<StatManager>("StatManager");
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

        AudioStream? deathStream = DeathSound ?? GD.Load<AudioStream>("res://Assets/Audio/kenney_impact-sounds/Audio/impactMining_001.ogg");
        if (deathStream != null)
        {
            AudioManager.Instance?.PlaySound2D(deathStream, GlobalPosition);
        }


        QueueFree();
    }
    protected override void OnDamageTaken(Node2D p_attacker)
    {
        base.OnDamageTaken(p_attacker);

        if (Stats.GetCurrentValue(StatType.Health) > 0)
        {
            AudioStream? hurtStream = HurtSound ?? GD.Load<AudioStream>("res://Assets/Audio/kenney_impact-sounds/Audio/scottishperson-sound-effect-woman-scream-236488.mp3");
            if (hurtStream != null)
            {
                AudioManager.Instance?.PlaySound2D(hurtStream, GlobalPosition);
            }
        }
    }
}
