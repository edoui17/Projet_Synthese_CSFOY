using Godot;
using System;
using Core.Managers.Stats;
using IslandSurvivor.Nodes;
using Core.Interfaces;

public partial class InteractionScript : Area2D
{
    private bool m_isPlayerInRange = false;
    private Node m_playerNode = null;

    // UI elements
    [Export] private Control m_uiLayer;

    // Progression variables to manage cost scaling
    private int m_healthUpgradeCount = 0;
    private int m_speedUpgradeCount = 0;
    private int m_attackUpgradeCount = 0;
    private int m_luckUpgradeCount = 0;

    private IShopManager m_shopManager;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        m_shopManager = new Core.Managers.ShopManager();

        m_uiLayer.Visible = false;
    }

    private void OnBodyEntered(Node p_body)
    {
        // Ideally we check for a group or script type. Since the player node is typically CharacterBody2D in our base map:
        if (p_body is CharacterBody2D || p_body.Name.ToString().Contains("Player", StringComparison.OrdinalIgnoreCase))
        {
            GD.Print($"Joueur est entré dans l'InteractionArea (Base) : {p_body.Name}. Shop opened.");
            m_isPlayerInRange = true;
            m_playerNode = p_body;
            m_uiLayer.Visible = true;
        }
    }

    private void OnBodyExited(Node p_body)
    {
        if (p_body == m_playerNode)
        {
            GD.Print($"Joueur est sorti de l'InteractionArea (Base) : {p_body.Name}. Shop closed.");
            m_isPlayerInRange = false;
            m_playerNode = null;
            m_uiLayer.Visible = false;
        }
    }

    private void TryPurchaseUpgrade(string p_resourceId, StatType p_statType, ref int p_upgradeCount)
    {
        int cost = m_shopManager.CalculateCost(p_upgradeCount);

        if (m_shopManager.CanAffordUpgrade(InventoryNode.Instance.Manager, p_resourceId, cost))
        {
            // Spend the resource
            SignalManager.Instance.EmitResourceSpent(this, p_resourceId, cost);

            // Signal the stat increase
            SignalManager.Instance.EmitStatUpgradePurchased(this, p_statType);

            p_upgradeCount++;
            GD.Print($"[Interaction] Amélioration réussie ! Nouveau coût pour cette stat : {m_shopManager.CalculateCost(p_upgradeCount)} {p_resourceId}.");
        }
        else
        {
            int currentAmount = InventoryNode.Instance.Manager.GetMaterialCount(p_resourceId);
            GD.Print($"[Interaction] Pas assez de {p_resourceId} pour l'amélioration {p_statType}. Requis : {cost}, Actuel : {currentAmount}.");
        }
    }

    private void TryPurchaseIsland()
    {
        int cost = 1; // 1 of each for the island initially

        if (m_shopManager.CanAffordIsland(InventoryNode.Instance.Manager, cost))
        {
            SignalManager.Instance.EmitResourceSpent(this, "Viande", cost);
            SignalManager.Instance.EmitResourceSpent(this, "Bois", cost);
            SignalManager.Instance.EmitResourceSpent(this, "Roche", cost);
            SignalManager.Instance.EmitResourceSpent(this, "Or", cost);

            GD.Print($"[Interaction] Île achetée avec succès ! (Simulation)");
        }
        else
        {
            GD.Print($"[Interaction] Pas assez de ressources pour acheter l'île. 1 de chaque requis.");
        }
    }
}
