using Core.Interfaces.Stats;
using Core.Managers.Stats;
using Godot;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using System;

public partial class AutomnTree : Area2D, ITree, IDamageable
{
    [Export] public StatManager Stats { get; set; }
    [Export] public string EntityId { get; set; } = "tree_automn_01";
    [Export] public Timer Timer { get; set; }

    [Export] public string MaterialName { get; set; } = "Bois d'automne";
    [Export] public string MaterialType { get; set; } = "Wood";
    [Export] public string IconPath { get; set; } = "res://Assets/TinySwords(FreePack)/TinySwords(FreePack)/Terrain/Resources/Wood/Trees/Tree4.png";


    public override void _Ready()
    {
        if (Stats != null)
        {
            Stats.SetCurrentValue(StatType.Health, 1000);
        }
        AreaEntered += OnAreaEntered;
    }

    public void OnAreaEntered(Area2D p_area)
    {
        if (p_area.IsInGroup("Tool"))
        {
            if (Timer == null || Timer.IsStopped())
            {
                Timer?.Start();
                DestroyResource();
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

        var item = new Core.Domain.ResourceItem(EntityId, MaterialName, MaterialType, IconPath);
        SignalManager.Instance.EmitMaterialDestroyed(this, item, quantity);
        QueueFree();
    }

    public void TakeDamage(int p_amount, object p_attacker)
    {
        if (Stats == null) return;

        Stats.ModifyCurrentValue(StatType.Health, -p_amount);

        IslandSurvivor.Extensions.NodeExtensions.PlayHitFlash(this);

        if (Stats.GetCurrentValue(StatType.Health) <= 0)
        {
            DestroyResource();
        }
    }
}
