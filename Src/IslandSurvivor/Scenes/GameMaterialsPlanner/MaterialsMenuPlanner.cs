using Godot;
using System.Collections.Generic;
using Core.Managers.Stats;
using Core.Interfaces;

public partial class MaterialsMenuPlanner : Control
{
  [ExportGroup("Boutons d'Amélioration")]
  [Export] private Button _buyHealthBtn;
  [Export] private Button _buySpeedBtn;
  [Export] private Button _buyAttackBtn;
  [Export] private Button _buyLuckBtn;

  [ExportGroup("Boutons Spéciaux")]
  [Export] private Button _buyIslandBtn;

  // --- State ---
  private Dictionary<StatType, int> _upgradeCounts = new Dictionary<StatType, int>()
    {
        { StatType.Health, 0 },
        { StatType.Speed, 0 },
        { StatType.Attack, 0 },
        { StatType.Luck, 0 }
    };

  private IShopManager _shopManager;

  public override void _Ready()
  {
    _shopManager = new Core.Managers.ShopManager();

    // On s'assure que le menu est caché au lancement
    Visible = false;

    // Connexions des boutons
    if (_buyHealthBtn != null) _buyHealthBtn.Pressed += () => TryPurchaseUpgrade("Or", StatType.Health);
    if (_buySpeedBtn != null) _buySpeedBtn.Pressed += () => TryPurchaseUpgrade("Bois", StatType.Speed);
    if (_buyAttackBtn != null) _buyAttackBtn.Pressed += () => TryPurchaseUpgrade("Roche", StatType.Attack);
    if (_buyLuckBtn != null) _buyLuckBtn.Pressed += () => TryPurchaseUpgrade("Viande", StatType.Luck);
    if (_buyIslandBtn != null) _buyIslandBtn.Pressed += TryPurchaseIsland;
  }
  public void ToggleMenu()
  {
    Visible = !Visible;
    GD.Print($"[Menu] Affichage : {Visible}");
  }

  private void TryPurchaseUpgrade(string resourceId, StatType statType)
  {
    int currentLevel = _upgradeCounts[statType];
    int cost = _shopManager.CalculateCost(currentLevel);

    if (_shopManager.CanAffordUpgrade(InventoryNode.Instance.Manager, resourceId, cost))
    {
      SignalManager.Instance.EmitResourceSpent(this, resourceId, cost);
      SignalManager.Instance.EmitStatUpgradePurchased(this, statType);

      _upgradeCounts[statType]++;
      GD.Print($"[Menu] {statType} niveau {_upgradeCounts[statType]} acheté !");
    }
    else
    {
      GD.Print($"[Menu] Ressources insuffisantes pour {statType}.");
    }
  }

  private void TryPurchaseIsland()
  {
    // L'ouverture du menu est maintenant gratuite
    // 1. Trouver le NavigationMenu de façon robuste à partir de la racine
    var navMenu = GetTree().Root.GetNodeOrNull<IslandSurvivor.Scenes.NavigationMenu.NavigationMenu>("Main/BaseMapIsland/NavigationMenu");

    if (navMenu != null)
    {
      navMenu.OpenMenu();
      // On s'assure qu'il peut maintenant recevoir des clics
      navMenu.MouseFilter = MouseFilterEnum.Pass;
      GD.Print("[Shop] NavigationMenu activé !");
    }
    else
    {
      GD.PrintErr("[Shop] Impossible de trouver le NavigationMenu.");
    }

    // 2. Fermer le shop
    Visible = false;
  }
}