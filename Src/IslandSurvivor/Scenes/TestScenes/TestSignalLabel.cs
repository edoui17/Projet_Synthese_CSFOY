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

    // Utilisation de la méthode correcte définie dans WeakEvent.cs
    SignalManager.Instance.OnMaterialDestroyed.AddListener(OnMaterialCollected);

    Text = "En attente de récolte...";
    GD.Print("TestSignalLabel: Prêt et abonné.");
  }

  private void OnMaterialCollected(object sender, ISignalManager.MaterialDestroyedEventArgs e)
  {
    // Mise à jour sécurisée de l'UI Godot
    CallDeferred(MethodName.UpdateVisuals, e.Item.Name, e.MaterialQuantity);
  }

  private void UpdateVisuals(string itemName, int quantity)
  {
    Text = $"Récolté : {quantity}x {itemName}";
    GD.Print($"[SIGNAL] Affichage mis à jour : {itemName} x{quantity}");
  }

  public override void _ExitTree()
  {
    // Nettoyage manuel (même si WeakEvent gère les références mortes)
    SignalManager.Instance?.OnMaterialDestroyed.RemoveListener(OnMaterialCollected);
  }
}