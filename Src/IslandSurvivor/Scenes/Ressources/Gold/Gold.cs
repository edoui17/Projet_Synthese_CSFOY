using Core.Domain;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Classes;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using System;

public partial class Gold : Area2D, IOre, IDamageable
{
    [Export] public StatManager Stats { get; set; } 
    [Export] public string EntityId { get; set; } = "gold_01";
    [Export] public Timer Timer { get; set; }

    [Export] public string MaterialName { get; set; } = "Or";
    [Export] public string MaterialType { get; set; } = "Gold";
    [Export] public string IconPath { get; set; } = "sera a valider";

    

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

    public void DestroyResource(object p_attacker = null)
    {
        Random random = new();
        int quantity = random.Next(1, 5);

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

        quantity += bonusQuantity;

        ResourceItem item = new ResourceItem(EntityId, MaterialName, MaterialType, IconPath);

        SignalManager.Instance.EmitMaterialDestroyed(this, item, quantity);
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
        Stats.ModifyCurrentValue(StatType.Health, - p_amount);

        IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);
    }
}
