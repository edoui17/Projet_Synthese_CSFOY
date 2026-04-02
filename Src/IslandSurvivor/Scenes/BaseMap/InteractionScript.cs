using Godot;
using System;
using Core.Managers.Stats;
using IslandSurvivor.Nodes;
using Core.Interfaces;

public partial class InteractionScript : Area2D
{
    private bool m_isPlayerInRange = false;
    private Node m_playerNode = null;

    // Progression variables to manage cost scaling
    private int m_healthUpgradeCount = 0;
    private int m_speedUpgradeCount = 0;
    private int m_attackUpgradeCount = 0;
    private int m_luckUpgradeCount = 0;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node p_body)
    {
        // Ideally we check for a group or script type. Since the player node is typically CharacterBody2D in our base map:
        if (p_body is CharacterBody2D || p_body.Name.ToString().Contains("Player", StringComparison.OrdinalIgnoreCase))
        {
            GD.Print($"Joueur est entré dans l'InteractionArea (Base) : {p_body.Name}. Appuyez sur E puis 1,2,3,4,5 pour tester les achats.");
            m_isPlayerInRange = true;
            m_playerNode = p_body;
        }
    }

    private void OnBodyExited(Node p_body)
    {
        if (p_body == m_playerNode)
        {
            GD.Print($"Joueur est sorti de l'InteractionArea (Base) : {p_body.Name}");
            m_isPlayerInRange = false;
            m_playerNode = null;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!m_isPlayerInRange || m_playerNode == null)
        {
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Key1: // Scenario 1: Viande -> Vie
                    TryPurchaseUpgrade("Viande", StatType.Health, ref m_healthUpgradeCount);
                    break;
                case Key.Key2: // Scenario 2: Bois -> Vitesse
                    TryPurchaseUpgrade("Bois", StatType.Speed, ref m_speedUpgradeCount);
                    break;
                case Key.Key3: // Scenario 3: Roche -> Force (Attack)
                    TryPurchaseUpgrade("Roche", StatType.Attack, ref m_attackUpgradeCount);
                    break;
                case Key.Key4: // Scenario 4: Or -> Chance
                    TryPurchaseUpgrade("Or", StatType.Luck, ref m_luckUpgradeCount);
                    break;
                case Key.Key5: // Scenario 5: Buy Island
                    TryPurchaseIsland();
                    break;
            }
        }
    }

    private int CalculateCost(int p_upgradeCount)
    {
        // Cost scaling: 1, 3, 7, 15... (2^(n+1) - 1)
        return (int)Math.Pow(2, p_upgradeCount + 1) - 1;
    }

    private void TryPurchaseUpgrade(string p_resourceId, StatType p_statType, ref int p_upgradeCount)
    {
        int cost = CalculateCost(p_upgradeCount);
        int currentAmount = InventoryNode.Instance.Manager.GetMaterialCount(p_resourceId);

        if (currentAmount >= cost)
        {
            // Spend the resource
            SignalManager.Instance.EmitResourceSpent(this, p_resourceId, cost);

            // Add stat bonus to player
            StatManager statManager = GetPlayerStatManager();
            if (statManager != null)
            {
                statManager.AddPermanentBonus(p_statType, 1f); // +1 stat point
                p_upgradeCount++;
                GD.Print($"[Interaction] Amélioration réussie ! +1 {p_statType}. Nouveau coût pour cette stat : {CalculateCost(p_upgradeCount)} {p_resourceId}.");
            }
            else
            {
                GD.PrintErr($"[Interaction] Impossible de trouver le StatManager sur le joueur.");
            }
        }
        else
        {
            GD.Print($"[Interaction] Pas assez de {p_resourceId} pour l'amélioration {p_statType}. Requis : {cost}, Actuel : {currentAmount}.");
        }
    }

    private void TryPurchaseIsland()
    {
        int cost = 1; // 1 of each for the island initially

        int meatCount = InventoryNode.Instance.Manager.GetMaterialCount("Viande");
        int woodCount = InventoryNode.Instance.Manager.GetMaterialCount("Bois");
        int rockCount = InventoryNode.Instance.Manager.GetMaterialCount("Roche");
        int goldCount = InventoryNode.Instance.Manager.GetMaterialCount("Or");

        if (meatCount >= cost && woodCount >= cost && rockCount >= cost && goldCount >= cost)
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

    private StatManager GetPlayerStatManager()
    {
        if (m_playerNode != null)
        {
            return m_playerNode.GetNodeOrNull<StatManager>("StatsManager");
        }
        return null;
    }
}
