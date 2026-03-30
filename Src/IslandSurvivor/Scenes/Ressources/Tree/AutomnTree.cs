using Godot;
using IslandSurvivor.Interfaces;
using System;

public partial class ArbreAutomne : Area2D, ITree
{
    public string m_materialName => throw new NotImplementedException();

    public string m_materialType => throw new NotImplementedException();

    public string m_entityId => throw new NotImplementedException();

    public Timer m_timer => throw new NotImplementedException();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void OnAreaEntered(Area2D area)
    {
        throw new NotImplementedException();
    }

    public void DestroyRessource()
    {
        Random random = new();
        int quantity = random.Next(1, 5);

        EmitSignal(SignalName.GoldBroken, quantity);
        QueueFree();
    }
}
