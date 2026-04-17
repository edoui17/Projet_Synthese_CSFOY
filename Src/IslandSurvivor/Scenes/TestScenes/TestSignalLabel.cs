using Core.Interfaces;
using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;
// Résolution de l'ambiguïté pour Godot.Label
using Label = Godot.Label;

public partial class TestSignalLabel : Label
{
  public override void _Ready()
  {
    if (SignalManager.Instance == null)
    {
      GD.PrintErr("TestSignalLabel: SignalManager introuvable dans les Autoloads.");
      return;
    }

    // Utilisation des signaux natifs
    SignalManager.Instance.MaterialDestroyed += OnMaterialCollected;

    Text = "En attente de récolte...";
    GD.Print("TestSignalLabel: Prêt et abonné.");
  }

  private void OnMaterialCollected(string p_itemId, string p_itemName, string p_itemType, string p_itemIcon, int p_quantity)
  {
    // Mise à jour sécurisée de l'UI Godot
    CallDeferred(MethodName.UpdateVisuals, p_itemName, p_quantity);
  }

  private void UpdateVisuals(string itemName, int quantity)
  {
    Text = $"Récolté : {quantity}x {itemName}";
    GD.Print($"[SIGNAL] Affichage mis à jour : {itemName} x{quantity}");
  }

  public override void _ExitTree()
  {
    if (SignalManager.Instance != null)
    {
      SignalManager.Instance.MaterialDestroyed -= OnMaterialCollected;
    }
  }
}