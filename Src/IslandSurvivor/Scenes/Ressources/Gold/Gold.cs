using Godot;
using IslandSurvivor.Classes;
using IslandSurvivor.Interfaces;
using System;

public partial class Gold : Area2D, IOre
{
    [Signal] public delegate void GoldBrokenEventHandler(int quantity);

    [Export] public string EntityId { get; set; } = "gold_01";
    [Export] public Timer Timer { get; set; }

    [Export] public string MaterialName { get; set; } = "Or";
    [Export] public string MaterialType { get; set; } = "Gold";



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

        EmitSignal(SignalName.GoldBroken, quantity);
        QueueFree();
    }
    void IGatheringMaterials.OnAreaEntered(Area2D area)
    {
        if (m_timer.IsStopped() && area.IsInGroup("Tool"))
        {
            //StatManager.Instance.ApplyDamage(this, 1);
            m_timer.Start();
        }
    }
}
