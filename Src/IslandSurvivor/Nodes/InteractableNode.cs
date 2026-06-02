using Godot;
using IslandSurvivor.Interfaces;

namespace IslandSurvivor.Nodes;

public partial class InteractableNode : Area2D, IInteractable
{
    [Signal]
    public delegate void InteractedEventHandler();

    [Export] public string InteractionPrompt { get; set; } = "Press E to Interact";
    [Export] public bool IsInteractable { get; set; } = true;

    public float GetDistanceSquaredTo(float p_x, float p_y)
    {
        return GlobalPosition.DistanceSquaredTo(new Vector2(p_x, p_y));
    }

    public virtual void Interact()
    {
        GD.Print("Interacting with " + Name);
        EmitSignal(SignalName.Interacted);
    }
}
