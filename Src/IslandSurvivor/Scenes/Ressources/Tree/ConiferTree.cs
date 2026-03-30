using Godot;
using IslandSurvivor.Interfaces;
using System;

public partial class ConiferTree : Area2D, ITree
{
    [Signal] public delegate void ConiferTreeBrokenEventHandler(int quantity);
    [Export] public string EntityId { get; set; } = "automn_Tree_01";
    [Export] public Timer Timer { get; set; }

    [Export] public string MaterialName { get; set; } = "AutomnTree";
    [Export] public string MaterialType { get; set; } = "Tree";

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }


    public void OnAreaEntered(Area2D area)
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

        EmitSignal(SignalName.ConiferTree, quantity);
        QueueFree();
    }
}
