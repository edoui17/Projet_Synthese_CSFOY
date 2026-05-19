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

public partial class Rock : Area2D, IOre, IDamageable
{
    [Export] public StatManager Stats { get; set; } = null!;

    [Export] public string EntityId { get; set; } = "rock_01";
    [Export] public Timer Timer { get; set; } = null!;

    [Export] public string MaterialName { get; set; } = "Roche";
    [Export] public string MaterialType { get; set; } = "Rock";
    [Export] public string IconPath { get; set; } = "res://Assets/TinySwords/TinySwords(Update010)/Deco/06.png";

    private object? m_lastAttacker;

    public override void _Ready()
    {
        if (Stats != null)
        {
            Stats.SetCurrentValue(StatType.Health, 20);
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
                // If it's a Godot Node, we can pass it as the attacker
                DestroyResource(p_area.GetParent() ?? p_area);
            }
        }
    }

    public void DestroyResource(object? p_attacker = null)
    {
        Random random = new();
        int baseQuantity = random.Next(1, 5);
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
        AudioStream destroyStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/rock_destroy.wav");
        if (destroyStream != null)
        {
            AudioManager.Instance?.PlaySound2D(destroyStream, GlobalPosition);
        }

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
        AudioStream impactStream = GD.Load<AudioStream>("res://Assets/Sounds/Combat/rock_impact.wav");
        if (impactStream != null)
        {
            AudioManager.Instance?.PlaySound2D(impactStream, GlobalPosition);
        }
    }
}
