using Godot;
using IslandSurvivor.Classes;
using IslandSurvivor.Interfaces;
using System;

public partial class Rock : Area2D, IOre
{
    [Export] public string EntityId { get; set; } = "rock_01";
    [Export] public Timer Timer { get; set; }

    [Export] public string MaterialName { get; set; } = "Roche";
    [Export] public string MaterialType { get; set; } = "Rock";
    [Export] public string IconPath { get; set; } = "res://Assets/TinySwords/TinySwords(Update010)/Deco/06.png";

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D p_area)
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

        var item = new Core.Domain.ResourceItem(EntityId, MaterialName, MaterialType, IconPath);
        SignalManager.Instance.EmitMaterialDestroyed(this, item, quantity);
        QueueFree();
    }

    void IGatheringMaterials.OnAreaEntered(Area2D p_area)
    {
        OnAreaEntered(p_area);
    }
}
