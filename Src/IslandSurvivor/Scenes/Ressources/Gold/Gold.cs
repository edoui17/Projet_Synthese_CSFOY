using Godot;
using IslandSurvivor.Classes;
using System;

public partial class Gold : Area2D, Ore
{
    [Signal] public delegate void GoldBrokenEventHandler(int quantity);

    [Export] public string m_entityId { get; set; } = "gold";
    [Export] public Timer m_timer { get; set; }



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
    void Ore.OnAreaEntered(Area2D area)
    {
        OnAreaEntered(area);
    }

    public void DestroyOre()
    {
        Random random = new();
        int quantity = random.Next(1, 5);

        EmitSignal(SignalName.GoldBroken, quantity);
        QueueFree();
    }

}
