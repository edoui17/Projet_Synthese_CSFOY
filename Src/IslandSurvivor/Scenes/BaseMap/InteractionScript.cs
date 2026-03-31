using Godot;
using System;

public partial class InteractionScript : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        GD.Print($"Quelqu'un est entré dans l'InteractionArea : {body.Name}");
    }

    private void OnBodyExited(Node body)
    {
        GD.Print($"Quelqu'un est sorti de l'InteractionArea : {body.Name}");
    }
}
