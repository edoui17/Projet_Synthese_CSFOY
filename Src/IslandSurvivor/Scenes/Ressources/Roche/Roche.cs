using Godot;
using System;

public partial class Roche : Area2D
{
    [Signal] public delegate void RockBrokenEventHandler(int quantity);
    [Export] public string m_entityId = "rock";
    [Export] public Timer m_timer;

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

    public void DestroyRock()
    {
        Random random = new();
        int quantity = random.Next(1, 5);

        EmitSignal(SignalName.RockBroken, quantity);
        QueueFree();
    }
}
