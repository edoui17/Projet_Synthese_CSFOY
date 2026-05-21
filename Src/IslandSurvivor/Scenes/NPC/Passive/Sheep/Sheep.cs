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
    [Export] public string HurtSoundKey { get; set; } = "Sheep_Hurt";
    [Export] public float HurtSoundVolume { get; set; } = 0f;
    [Export] public string DeathSoundKey { get; set; } = "Resource_Mining_1";
    [Export] public float DeathSoundVolume { get; set; } = 0f;
    [Export] public string IdleSoundKey { get; set; } = "Sheep_Idle";
    [Export] public float IdleSoundVolume { get; set; } = 0f;

    private Timer? m_idleSoundTimer;

    public override void _Ready()
    {
        base._Ready();
        IdleSpeed = 30.0f;

        m_idleSoundTimer = new Timer();
        m_idleSoundTimer.WaitTime = new Random().Next(5, 30);
        m_idleSoundTimer.OneShot = false;
        m_idleSoundTimer.Timeout += OnIdleSoundTimeout;
        AddChild(m_idleSoundTimer);
        m_idleSoundTimer.Start();
    }

    private void OnIdleSoundTimeout()
    {
        if (!string.IsNullOrEmpty(IdleSoundKey) && CurrentState != NpcStates.DEAD)
        {
            AudioManager.Instance?.PlaySound2D(IdleSoundKey, GlobalPosition, IdleSoundVolume);
            m_idleSoundTimer!.WaitTime = new Random().Next(10, 30);
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

        AudioManager.Instance?.PlaySound2D(DeathSoundKey, GlobalPosition, DeathSoundVolume);


        QueueFree();
    }
    protected override void OnDamageTaken(Node2D p_attacker)
    {
        base.OnDamageTaken(p_attacker);

        if (Stats.GetCurrentValue(StatType.Health) > 0)
        {
            AudioManager.Instance?.PlaySound2D(HurtSoundKey, GlobalPosition, HurtSoundVolume);
        }
    }
}
