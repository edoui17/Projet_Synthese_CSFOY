using Godot;
using System.Collections.Generic;
using Core.Managers.Stats;
using Core.Interfaces;

public partial class MaterialsMenuPlanner : Control
{
    [ExportGroup("Boutons d'Amélioration")]
    [Export] private Button _buyHealthBtn = null!;
    [Export] private Button _buySpeedBtn = null!;
    [Export] private Button _buyAttackBtn = null!;
    [Export] private Button _buyLuckBtn = null!;

    [ExportGroup("Boutons Spéciaux")]
    [Export] private Button _buyIslandBtn = null!;

    // --- State ---
    private Dictionary<StatType, int> _upgradeCounts = new Dictionary<StatType, int>()
    {
        { StatType.Health, 0 },
        { StatType.Speed, 0 },
        { StatType.Attack, 0 },
        { StatType.Luck, 0 }
    };

    private IShopManager _shopManager = null!;

    public override void _Ready()
    {
        _shopManager = IslandSurvivor.Globals.ServiceRegistry.Instance.ShopManager;

        // On s'assure que le menu est caché au lancement
        Visible = false;

        // Connexions des boutons
        if (_buyHealthBtn != null) _buyHealthBtn.Pressed += () => TryPurchaseUpgrade("meat_01", StatType.Health);
        if (_buySpeedBtn != null) _buySpeedBtn.Pressed += () => TryPurchaseUpgrade("wood_01", StatType.Speed);
        if (_buyAttackBtn != null) _buyAttackBtn.Pressed += () => TryPurchaseUpgrade("rock_01", StatType.Attack);
        if (_buyLuckBtn != null) _buyLuckBtn.Pressed += () => TryPurchaseUpgrade("gold_01", StatType.Luck);
        if (_buyIslandBtn != null) _buyIslandBtn.Pressed += TryPurchaseIsland;

        SignalManager.Instance.BuildingShopToggled += OnBuildingShopToggled;
    }

    protected override void Dispose(bool p_disposing)
    {
        if (p_disposing)
        {
            if (SignalManager.Instance != null)
            {
                SignalManager.Instance.BuildingShopToggled -= OnBuildingShopToggled;
            }
        }
        base.Dispose(p_disposing);
    }

    private void OnBuildingShopToggled(bool p_isOpen, string p_buildingId)
    {
        Visible = p_isOpen;
        GD.Print($"[Menu] Shop Toggled: {Visible} (Building: {p_buildingId})");
    }

    public void ToggleMenu()
    {
        Visible = !Visible;
        GD.Print($"[Menu] Affichage : {Visible}");
    }

    private void TryPurchaseUpgrade(string p_resourceId, StatType p_statType)
    {
        int currentLevel = _upgradeCounts[p_statType];
        int cost = _shopManager.CalculateCost(currentLevel);

        if (_shopManager.CanAffordUpgrade(InventoryNode.Instance.Manager, p_resourceId, cost))
        {
            SignalManager.Instance.EmitResourceSpent(this, p_resourceId, cost);
            SignalManager.Instance.EmitStatUpgradePurchased(this, p_statType);

            _upgradeCounts[p_statType]++;
            GD.Print($"[Menu] {p_statType} niveau {_upgradeCounts[p_statType]} acheté !");
        }
        else
        {
            GD.Print($"[Menu] Ressources insuffisantes pour {p_statType}.");
        }
    }

    private void TryPurchaseIsland()
    {
        // L'ouverture du menu est maintenant gratuite
        // 1. Trouver le NavigationMenu de façon robuste à partir de la racine
        var navMenu = GetTree().Root.GetNodeOrNull<IslandSurvivor.Scenes.NavigationMenu.NavigationMenu>("Main/CanvasLayer/NavigationMenu");

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