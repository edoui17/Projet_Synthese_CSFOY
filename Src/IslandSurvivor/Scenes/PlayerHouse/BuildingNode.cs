using Godot;
using System;
using IslandSurvivor.Interfaces;

public partial class BuildingNode : Node2D, IInteractable
{
  [Export] private Area2D _interactionArea;

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
    // emettre un signal
  }

  public float GetDistanceTo(float p_x, float p_y)
  {
    return GlobalPosition.DistanceTo(new Vector2(p_x, p_y));
  }

  private void OnBodyExited(Node body)
  {
    // emttre un signal
  }
}