namespace IslandSurvivor.Scenes.NPC.Passive;

using IslandSurvivor.Logic.Entities;
using Core.Domain;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Globals;
using IslandSurvivor.Nodes;
using System;

public partial class Sheep : PassiveNpcBase
{
    [ExportGroup("Audio Override")]
    [Export] public AudioStream? IdleSound { get; set; }
    [Export] public string IdleSoundKey { get; set; } = "Sheep_Idle";
    [Export] public float IdleVolume { get; set; } = 1.0f;

    private Timer? m_idleSoundTimer;

    public override void _Ready()
    {
        base._Ready();
        IdleSpeed = 30.0f;

        // Initialize default keys if not set
        if (string.IsNullOrEmpty(HurtSoundKey)) HurtSoundKey = "Sheep_Hurt";
        if (string.IsNullOrEmpty(DeathSoundKey)) DeathSoundKey = "Resource_Mining_1";
        if (AudioMaxDistance >= 2000f) AudioMaxDistance = 275f; // Sheep specific default

        m_idleSoundTimer = new Timer();
        m_idleSoundTimer.WaitTime = new Random().Next(5, 20);
        m_idleSoundTimer.OneShot = false;
        m_idleSoundTimer.Timeout += OnIdleSoundTimeout;
        AddChild(m_idleSoundTimer);
        m_idleSoundTimer.Start();
    }

    private void OnIdleSoundTimeout()
    {
        if (CurrentState != NpcStates.DEAD)
        {
            if (IdleSound != null)
                AudioManager.Instance?.PlaySound2D(IdleSound, GlobalPosition, p_volumeLinear: IdleVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
            else if (!string.IsNullOrEmpty(IdleSoundKey))
                AudioManager.Instance?.PlaySound2D(IdleSoundKey, GlobalPosition, p_volumeLinear: IdleVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);

            m_idleSoundTimer!.WaitTime = new Random().Next(5, 20);
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

        if (DeathSound != null)
            AudioManager.Instance?.PlaySound2D(DeathSound, GlobalPosition, p_volumeLinear: DeathVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
        else if (!string.IsNullOrEmpty(DeathSoundKey))
            AudioManager.Instance?.PlaySound2D(DeathSoundKey, GlobalPosition, p_volumeLinear: DeathVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);


        QueueFree();
    }
    protected override void OnDamageTaken(Node2D p_attacker)
    {
        base.OnDamageTaken(p_attacker);

        if (Stats.GetCurrentValue(StatType.Health) > 0)
        {
            if (HurtSound != null)
                AudioManager.Instance?.PlaySound2D(HurtSound, GlobalPosition, p_volumeLinear: HurtVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
            else if (!string.IsNullOrEmpty(HurtSoundKey))
                AudioManager.Instance?.PlaySound2D(HurtSoundKey, GlobalPosition, p_volumeLinear: HurtVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
        }
    }
}
