using Godot;
using System;
using IslandSurvivor.Interfaces;

public partial class BuildingNode : Node2D, IInteractable
{
  // On utilise UNIQUEMENT l'export pour le lien
  [Export] private MaterialsMenuPlanner _menu;
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
    if (_menu != null)
    {
      _menu.ToggleMenu();
    }
    else
    {
      GD.PushError("BuildingNode: Le menu n'est pas lié dans l'inspecteur !");
    }
  }

  public float GetDistanceTo(float p_x, float p_y)
  {
    return GlobalPosition.DistanceTo(new Vector2(p_x, p_y));
  }

  private void OnBodyExited(Node body)
  {
    if (body.Name.ToString().Contains("Player", StringComparison.OrdinalIgnoreCase))
    {
      if (_menu != null) _menu.Visible = false;
    }
  }
}