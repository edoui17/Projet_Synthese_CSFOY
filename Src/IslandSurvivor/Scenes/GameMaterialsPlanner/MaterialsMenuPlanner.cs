using Godot;
using System.Collections.Generic;
using Core.Managers;
using Core.Interfaces;

public partial class MaterialsMenuPlanner : Control
{
    private static readonly string RESOURCE_MEAT = "meat_01";
    private static readonly string RESOURCE_WOOD = "wood_01";
    private static readonly string RESOURCE_ROCK = "rock_01";
    private static readonly string RESOURCE_GOLD = "gold_01";

    [ExportGroup("Boutons d'Amélioration")]
    [Export] private Button m_buyHealthBtn = null!;
    [Export] private Button m_buySpeedBtn = null!;
    [Export] private Button m_buyAttackBtn = null!;
    [Export] private Button m_buyLuckBtn = null!;

    [ExportGroup("Boutons Spéciaux")]
    [Export] private Button m_buyIslandBtn = null!;

    // --- State ---
    private Dictionary<StatType, int> m_upgradeCounts = new Dictionary<StatType, int>()
    {
        { StatType.Health, 0 },
        { StatType.Speed, 0 },
        { StatType.Attack, 0 },
        { StatType.Luck, 0 }
    };

    private IShopManager m_shopManager = null!;

    public override void _Ready()
    {
        m_shopManager = IslandSurvivor.Globals.ServiceRegistry.Instance.ShopManager;

        // On s'assure que le menu est caché au lancement
        Visible = false;

        // Connexions des boutons
        if (m_buyHealthBtn != null) m_buyHealthBtn.Pressed += () => TryPurchaseUpgrade(RESOURCE_MEAT, StatType.Health);
        if (m_buySpeedBtn != null) m_buySpeedBtn.Pressed += () => TryPurchaseUpgrade(RESOURCE_WOOD, StatType.Speed);
        if (m_buyAttackBtn != null) m_buyAttackBtn.Pressed += () => TryPurchaseUpgrade(RESOURCE_ROCK, StatType.Attack);
        if (m_buyLuckBtn != null) m_buyLuckBtn.Pressed += () => TryPurchaseUpgrade(RESOURCE_GOLD, StatType.Luck);
        if (m_buyIslandBtn != null) m_buyIslandBtn.Pressed += TryPurchaseIsland;

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
        int currentLevel = m_upgradeCounts[p_statType];
        int cost = m_shopManager.CalculateCost(currentLevel);

        if (!m_shopManager.CanAffordUpgrade(InventoryNode.Instance.Manager, p_resourceId, cost))
        {
            GD.Print($"[Menu] Ressources insuffisantes pour {p_statType}.");
            return;
        }

        SignalManager.Instance.EmitResourceSpent(this, p_resourceId, cost);
        SignalManager.Instance.EmitStatUpgradePurchased(this, p_statType);

        m_upgradeCounts[p_statType]++;
        GD.Print($"[Menu] {p_statType} niveau {m_upgradeCounts[p_statType]} acheté !");
    }

    private void TryPurchaseIsland()
    {
        // L'ouverture du menu est maintenant gratuite
        // 1. Trouver le NavigationMenu de façon robuste à partir de la racine
        var navMenu = GetTree().Root.GetNodeOrNull<IslandSurvivor.Scenes.NavigationMenu.NavigationMenu>("Main/CanvasLayer/NavigationMenu");

        if (navMenu == null)
        {
            GD.PrintErr("[Shop] Impossible de trouver le NavigationMenu.");
            Visible = false;
            return;
        }

        navMenu.OpenMenu();
        // On s'assure qu'il peut maintenant recevoir des clics
        navMenu.MouseFilter = MouseFilterEnum.Pass;
        GD.Print("[Shop] NavigationMenu activé !");

        // 2. Fermer le shop
        Visible = false;
    }
}