using Godot;
using System;

public partial class Spawn : Marker2D
{
    // Called when the node enters the scene tree for the first time.
    [Export] public Marker2D SpawnPoint { get; set; }
    [Export] public PackedScene PlayerScene { get; set; }

    public override void _Ready()
	{
        if (PlayerScene == null)
        {
            GD.PrintErr("PlayerScene n'est pas assigné !");
            return;
        }

        if (SpawnPoint == null)
        {
            GD.PrintErr("SpawnPoint n'est pas assigné !");
            return;
        }

 
        CallDeferred(nameof(SpawnPlayer));
    }
    private void SpawnPlayer()
    {
        var player = PlayerScene.Instantiate<Node2D>();
        player.Position = SpawnPoint.Position;

        GetTree().CurrentScene.AddChild(player);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
