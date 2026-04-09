using Godot;
using System;
using Core.Managers.Stats;
using IslandSurvivor.Nodes;
using Core.Interfaces;

using IslandSurvivor.Interfaces;

public partial class InteractionScript : Area2D, IInteractable
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

    public bool IsInteractable => true;
    public string InteractionPrompt => "Press E to open Shop";

    public override void _Ready()
    {
        BodyExited += OnBodyExited;
        m_shopManager = new Core.Managers.ShopManager();
        m_uiLayer.Visible = false;
    }

    public float GetDistanceTo(float p_x, float p_y)
    {
        return GlobalPosition.DistanceTo(new Vector2(p_x, p_y));
    }

    public void Interact()
    {
        GD.Print("[Interaction] Player opened the Shop.");
        m_uiLayer.Visible = true;
    }

    private void OnBodyExited(Node p_body)
    {
        if (p_body is CharacterBody2D || p_body.Name.ToString().Contains("Player", StringComparison.OrdinalIgnoreCase))
        {
            GD.Print($"Joueur est sorti de l'InteractionArea (Base) : {p_body.Name}. Shop closed.");
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
