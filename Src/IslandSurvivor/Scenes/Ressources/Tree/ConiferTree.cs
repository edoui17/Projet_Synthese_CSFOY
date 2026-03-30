using Godot;
using System;

using IslandSurvivor.Interfaces;

public partial class ConiferTree : Area2D, ITree
{
    [Export] public string EntityId { get; set; } = "tree_conifer_01";
    [Export] public Timer Timer { get; set; }

    [Export] public string MaterialName { get; set; } = "Conifère";
    [Export] public string MaterialType { get; set; } = "Wood";

    private bool m_isPlayerNear = false;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    public void OnAreaEntered(Area2D p_area)
    {
        if (p_area.IsInGroup("Player"))
        {
            m_isPlayerNear = true;
        }
    }

    private void OnAreaExited(Area2D p_area)
    {
        if (p_area.IsInGroup("Player"))
        {
            m_isPlayerNear = false;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (m_isPlayerNear && @event.IsActionPressed("interact"))
        {
            if (Timer == null || Timer.IsStopped())
            {
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
}
