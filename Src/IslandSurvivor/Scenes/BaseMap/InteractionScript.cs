using Godot;
using IslandSurvivor.Interfaces;

namespace IslandSurvivor.Nodes;

public partial class InteractionScript : InteractableNode
{
    public override void _Ready()
    {
        InteractionPrompt = "Enter Building";
        base._Ready();
    }

    public override void Interact()
    {
        GD.Print($"Interacting with Building: {GetParent().Name}");
        base.Interact();
        // Additional building-specific logic could go here
    }
}
