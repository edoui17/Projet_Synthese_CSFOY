using Godot;
using System;

public partial class Or : Area2D
{
    [Signal] public delegate void GoldBrokenEventHandler(int quantity);
    [Export] public string EntityId = "gold";

    private double _damageCooldown = 0.2;
    private double _timerSinceLastDamage = 0.0;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (_timerSinceLastDamage <= 0 && area.IsInGroup("Tool"))
        {
            //StatManager.Instance.ApplyDamage(this, 1);
            _timerSinceLastDamage = _damageCooldown;
        }
    }
    public override void _Process(double delta)
    {
        if (_timerSinceLastDamage > 0) _timerSinceLastDamage -= delta;
    }

    public void DestroyRock()
    {
        Random random = new();
        int quantity = random.Next(1, 5);

        EmitSignal(SignalName.GoldBroken, quantity);
        QueueFree();
    }
}
