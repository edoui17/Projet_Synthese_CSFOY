using Godot;
using IslandSurvivor.Classes;
using IslandSurvivor.Interfaces;
using System;

public partial class Gold : Area2D, IOre
{
    [Export] public string EntityId { get; set; } = "gold_01";
    [Export] public Timer Timer { get; set; }

    [Export] public string MaterialName { get; set; } = "Or";
    [Export] public string MaterialType { get; set; } = "Gold";

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

        SignalManager.Instance.EmitMaterialDestroyed(this, MaterialType, quantity);
        QueueFree();
    }

    void IGatheringMaterials.OnAreaEntered(Area2D p_area)
    {
        OnAreaEntered(p_area);
    }
}
