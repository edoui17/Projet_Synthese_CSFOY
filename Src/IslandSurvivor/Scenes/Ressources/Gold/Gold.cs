using Core.Domain;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Extensions;
using IslandSurvivor.Globals;
using IslandSurvivor.Classes;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using System;

public partial class Gold : Area2D, IOre, IDamageable
{
    [Export] public StatManager Stats { get; set; } = null!;
    [Export] public string EntityId { get; set; } = "gold_01";
    [Export] public Timer Timer { get; set; } = null!;

    [Export] public string MaterialName { get; set; } = "Or";
    [Export] public string MaterialType { get; set; } = "Gold";
    [Export] public string IconPath { get; set; } = "";

    [ExportGroup("Audio")]
    [Export] public float AudioMaxDistance { get; set; } = 2000f;
    [Export] public float AudioAttenuation { get; set; } = 1f;
    [Export] public float ImpactVolume { get; set; } = 1.0f;
    [Export] public float DestroyVolume { get; set; } = 1.0f;
    public string NpcType { get; set; } = "Gold";

    private object? m_lastAttacker;

    public override void _Ready()
    {
        if (Stats != null)
        {
            Stats.SetCurrentValue(StatType.Health, 30);
            Stats.Connect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
        }
        AreaEntered += OnAreaEntered;
    }

    private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
    {
        if ((StatType)p_statType == StatType.Health && p_currentValue <= 0)
        {
            if (Stats != null)
            {
                Stats.Disconnect(StatManager.SignalName.LocalStatChanged, Callable.From<int, float, float>(OnStatChanged));
            }
            DestroyResource(m_lastAttacker);
        }
    }

    private void OnAreaEntered(Area2D p_area)
    {
        if (p_area.IsInGroup("Tool"))
        {
            if (Timer == null || Timer.IsStopped())
            {
                Timer?.Start();
                TakeDamage(10, p_area.GetParent() ?? p_area); // Example damage value, adjust as needed
            }
        }
    }

    public void DestroyResource(object? p_attacker = null)
    {
        int baseQuantity = (int)(GD.Randi() % 4) + 1;
        int quantity = IslandSurvivor.Logic.ResourceUtils.CalculateYield(baseQuantity, p_attacker);

        ResourceItem item = new ResourceItem(EntityId, MaterialName, MaterialType, IconPath);

        // Target is the player who mined it
        Node2D targetNode = (p_attacker as Node2D)!;
        Vector2 fallbackPosition = targetNode != null ? targetNode.GlobalPosition : GlobalPosition;

        // Spawn Resource Drops for tweening
        PackedScene dropScene = GD.Load<PackedScene>("res://Scenes/Ressources/ResourceDrop.tscn");
        if (dropScene != null)
        {
            for (int i = 0; i < quantity; i++)
            {
                if (dropScene.Instantiate() is IslandSurvivor.Scenes.Ressources.ResourceDrop drop)
                {
                    // Pass quantity 1 for each individual drop
                    drop.Initialize(item, 1, GlobalPosition, targetNode!, fallbackPosition);
                    GetParent().AddChild(drop);
                }
            }
        }
        else
        {
            // Fallback if scene is not yet setup
            SignalManager.Instance.EmitMaterialDestroyed(this, item, quantity);
        }

        // Detach and play particles if they exist
        GpuParticles2D particles = GetNodeOrNull<GpuParticles2D>("DestructionParticles");
        if (particles != null)
        {
            RemoveChild(particles);
            GetParent().AddChild(particles);
            particles.GlobalPosition = GlobalPosition;
            particles.Emitting = true;
            // Free particles after they finish (assume 2 seconds is enough)
            GetTree().CreateTimer(2.0f).Timeout += () =>
            {
                if (GodotObject.IsInstanceValid(particles))
                    particles.QueueFree();
            };
        }

        // Try to play destroy sound
        AudioManager.Instance?.PlaySound2D("Resource_Mining_1", GlobalPosition, p_volumeLinear: DestroyVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);

        QueueFree();
    }

    void IGatheringMaterials.OnAreaEntered(Area2D p_area)
    {
        OnAreaEntered(p_area);
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (Stats == null) return;

        m_lastAttacker = p_attacker;
        Stats.ModifyCurrentValue(StatType.Health, -p_amount);

        this.PlayHitFlash();
        this.PlayShake();

        // Try to play impact sound
        if (Stats.GetCurrentValue(StatType.Health) > 0)
        {
            AudioManager.Instance?.PlaySound2D("Resource_Mining_1", GlobalPosition, p_volumeLinear: ImpactVolume, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
        }
    }
}
