using Godot;
using System;

using IslandSurvivor.Interfaces;

public partial class ConiferTree : Area2D, ITree
{
    [Export] public string EntityId { get; set; } = "tree_conifer_01";
    [Export] public Timer Timer { get; set; }

    [Export] public string MaterialName { get; set; } = "Conifère";
    [Export] public string MaterialType { get; set; } = "Wood";
    [Export] public string IconPath { get; set; } = "res://Assets/Tiny Swords/Tiny Swords (Update 010)/Resources/Trees/Tree.png";

    public override void _Ready()
    {
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

        var item = new Core.Domain.ResourceItem(EntityId, MaterialName, MaterialType, IconPath);
        SignalManager.Instance.EmitMaterialDestroyed(this, item, quantity);
        QueueFree();
    }
}
