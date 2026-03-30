using Godot;
using IslandSurvivor.Classes;
using IslandSurvivor.Interfaces;
using System;

public partial class Rock : Area2D, IOre
{
    [Signal] public delegate void RockBrokenEventHandler(int quantity);

    [Export] public string m_entityId { get; set; } = "rock_01";
    [Export] public Timer m_timer { get; set; }

    [Export] public string m_materialName { get; set; } = "Roche";
    [Export] public string m_materialType { get; set; } = "Rock";

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (m_timer.IsStopped() && area.IsInGroup("Tool"))
        {
            //StatManager.Instance.ApplyDamage(this, 1);
            m_timer.Start();
        }
    }

    public void DestroyRessource()
    {
        Random random = new();
        int quantity = random.Next(1, 5);

        EmitSignal(SignalName.RockBroken, quantity);
        QueueFree();
    }

    void IGatheringMaterials.OnAreaEntered(Area2D area)
    {
        OnAreaEntered(area);
    }
}
