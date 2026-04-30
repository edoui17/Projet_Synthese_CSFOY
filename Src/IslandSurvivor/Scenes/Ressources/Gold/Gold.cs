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

    

    public override void _Ready()
    {
        if (Stats != null)
        {
            Stats.SetCurrentValue(StatType.Health, 5000);
        }
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D p_area)
    {
        if (p_area.IsInGroup("Tool"))
        {
            if (Timer == null || Timer.IsStopped())
            {
                Timer?.Start();
               TakeDamage(10, this); // Example damage value, adjust as needed
            }
        }
    }

    public void DestroyResource()
    {
        Random random = new();
        int quantity = random.Next(1, 5);

        float luck = IslandSurvivor.Globals.ServiceRegistry.Instance.StatTracker.GetCurrentValue(Core.Managers.Stats.StatType.Luck);
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

        Stats.ModifyCurrentValue(StatType.Health, - p_amount);

        IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);

        if (Stats.GetCurrentValue(StatType.Health) <= 0)
        {
            DestroyResource();
        }
    }
}
