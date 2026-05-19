using Godot;
using System;
using IslandSurvivor.Interfaces;

public partial class BuildingNode : Node2D, IInteractable
{
    [Export] private Area2D _interactionArea = null!;

    public bool IsInteractable => true;
    public string InteractionPrompt => "Appuyez sur [E] pour ouvrir la boutique";

    public override void _Ready()
    {

        if (_interactionArea != null)
        {
            _interactionArea.BodyExited += OnBodyExited;
        }
    }

    public void Interact()
    {
        SignalManager.Instance.EmitBuildingShopToggled(this, true, "PlayerHouse");
    }

    public float GetDistanceTo(float p_x, float p_y)
    {
        return GlobalPosition.DistanceTo(new Vector2(p_x, p_y));
    }

    private void OnBodyExited(Node body)
    {
        SignalManager.Instance.EmitBuildingShopToggled(this, false, "PlayerHouse");
    }
}