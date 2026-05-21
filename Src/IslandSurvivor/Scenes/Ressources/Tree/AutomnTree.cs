using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Extensions;
using IslandSurvivor.Globals;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using System;

public partial class AutomnTree : Area2D, ITree, IDamageable
{
    [Export] public StatManager Stats { get; set; } = null!;
    [Export] public string EntityId { get; set; } = "wood_01";
    [Export] public Timer Timer { get; set; } = null!;

    [Export] public string MaterialName { get; set; } = "Bois d'automne";
    [Export] public string MaterialType { get; set; } = "Wood";
    [Export] public string IconPath { get; set; } = "res://Assets/TinySwords(FreePack)/TinySwords(FreePack)/Terrain/Resources/Wood/Trees/Tree4.png";

    [ExportGroup("Audio")]
    [Export] public float AudioMaxDistance { get; set; } = 2000f;
    [Export] public float AudioAttenuation { get; set; } = 1f;

    private object? m_lastAttacker;

    public override void _Ready()
    {
        if (Stats != null)
        {
            Stats.SetCurrentValue(StatType.Health, 10);
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

    public void OnAreaEntered(Area2D p_area)
    {
        if (p_area.IsInGroup("Tool"))
        {
            if (Timer == null || Timer.IsStopped())
            {
                Timer?.Start();
                DestroyResource(p_area.GetParent() ?? p_area);
            }
        }
    }

    public void DestroyResource(object? p_attacker = null)
    {
        int baseQuantity = (int)(GD.Randi() % 4) + 1;
        int quantity = IslandSurvivor.Logic.ResourceUtils.CalculateYield(baseQuantity, p_attacker);

        var item = new Core.Domain.ResourceItem(EntityId, MaterialName, MaterialType, IconPath);
        SignalManager.Instance.EmitMaterialDestroyed(this, item, quantity);
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
        AudioManager.Instance?.PlaySound2D("Resource_Rustling", GlobalPosition, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);

        QueueFree();
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (Stats == null) return;

        m_lastAttacker = p_attacker;
        Stats.ModifyCurrentValue(StatType.Health, -p_amount);

        this.PlayHitFlash();
        this.PlayShake();

        //Try to play impact sound
        if (Stats.GetCurrentValue(StatType.Health) > 0)
        {
            AudioManager.Instance?.PlaySound2D("Impact_Wood_Light", GlobalPosition, p_maxDistance: AudioMaxDistance, p_attenuation: AudioAttenuation);
        }
    }
}
