using Godot;
using System;

public partial class Spawn : Marker2D
{
    // Called when the node enters the scene tree for the first time.
    [Export] public Marker2D SpawnPoint { get; set; }
    [Export] public CharacterBody2d Player { get; set; }

    public override void _Ready()
	{
        // var player = Instantiate<CharacterBody2d>(PlayerScene);
        Player.Position = SpawnPoint.Position;
        AddChild(Player);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
