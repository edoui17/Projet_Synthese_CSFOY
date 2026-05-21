using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Extensions;
using IslandSurvivor.Globals;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using System;

public partial class ConiferTree : Area2D, ITree, IDamageable
{
    [Export] public StatManager Stats { get; set; } = null!;
    [Export] public string EntityId { get; set; } = "wood_01";
    [Export] public Timer Timer { get; set; } = null!;

    [Export] public string MaterialName { get; set; } = "Conifère";
    [Export] public string MaterialType { get; set; } = "Wood";
    [Export] public string IconPath { get; set; } = "res://Assets/Tiny Swords/Tiny Swords (Update 010)/Resources/Trees/Tree.png";

    private object? m_lastAttacker;

    public override void _Ready()
    {
        if (Stats != null)
        {
            Stats.SetCurrentValue(StatType.Health, 40);
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
        
        AudioStream destroyStream = GD.Load<AudioStream>("res://Assets/Audio/kenney_impact-sounds/Audio/floraphonic-rustling-bushes-dried-leaves-2-230202.mp3");
        if (destroyStream != null)
        {
            AudioManager.Instance?.PlaySound2D(destroyStream, GlobalPosition);
        }

        QueueFree();
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (Stats == null) return;

        m_lastAttacker = p_attacker;
        Stats.ModifyCurrentValue(StatType.Health, -p_amount);

        AudioStream impactStream = null;
        if (Stats.GetCurrentValue(StatType.Health) > 0)
        {
            impactStream = GD.Load<AudioStream>("res://Assets/Audio/kenney_impact-sounds/Audio/impactWood_heavy_004.ogg");

        }
        if (impactStream != null)
        {
            AudioManager.Instance?.PlaySound2D(impactStream, GlobalPosition);
        }

        this.PlayHitFlash();
    }
}
