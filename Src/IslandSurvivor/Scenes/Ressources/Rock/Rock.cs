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

    private bool m_isPlayerNear = false;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    private void OnAreaEntered(Area2D p_area)
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

    void IGatheringMaterials.OnAreaEntered(Area2D p_area)
    {
        OnAreaEntered(p_area);
    }
}
