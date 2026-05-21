# Système de Magasin (ShopManager)

## Vue d'ensemble

Le système de magasin (`ShopManager`) gère l'économie et la logique d'achat dans *IslandSurvivor*. En respectant le pilier architectural **Accountant vs. Orchestrator**, le `ShopManager` réside entièrement dans le `Core` (`Src/Core/Managers/ShopManager.cs`) et ne dépend d'aucun élément visuel ou logique du moteur Godot.

Il agit comme le "Comptable" : il vérifie que les fonds sont suffisants, calcule les coûts de progression et réagit aux événements d'achat. Le client Godot (l'"Orchestrateur") se contente de l'interroger et de mettre à jour son interface (UI) en conséquence.

## 1. Méthodes Principales (`IShopManager`)

Le `ShopManager` expose plusieurs méthodes essentielles pour évaluer si une transaction est possible :

### `CalculateCost(int p_upgradeCount)`
Calcule le coût exponentiel d'une amélioration en fonction du nombre de fois qu'elle a déjà été achetée.
- **Formule :** $2^{(n+1)} - 1$
- *Exemple :* L'amélioration 0 coûte 1 ressource, la #1 coûte 3, la #2 coûte 7, etc.

### `CanAffordUpgrade(IInventoryManager p_inventoryManager, string p_resourceId, int p_cost)`
Vérifie si le joueur possède assez d'une ressource spécifique pour acheter une amélioration.
- Nécessite de passer une instance du gestionnaire d'inventaire (`IInventoryManager`).
- Interroge l'inventaire en utilisant `GetMaterialCount(p_resourceId)`.

### `CanAffordIsland(IInventoryManager p_inventoryManager, int p_cost)`
Vérifie si le joueur peut débloquer une nouvelle île. Cette action a un coût universel, signifiant que le joueur doit payer `p_cost` dans **toutes** les ressources majeures :
- Viande (`meat_01`)
- Bois (`wood_01`)
- Roche (`rock_01`)
- Or (`gold_01`)

## 2. Intégration avec l'EventBus

Le `ShopManager` s'abonne à des événements de jeu via le système **EventBus**, lui permettant de réagir à des actions sans être couplé directement à l'interface d'achat Godot.

Lors de son instanciation, il s'abonne à l'événement `TransactionResultEvent` :
```csharp
m_eventBus.Subscribe<TransactionResultEvent>(OnTransactionResult);
```
Cela permet au `ShopManager` de mettre à jour son état interne ou d'exécuter de la logique secondaire chaque fois qu'un achat réussit ou échoue depuis l'interface Godot.

## 3. Utilisation dans le Client Godot (Orchestrator)

Puisque le `ShopManager` est un pur gestionnaire C# injecté via le `ServiceRegistry`, les scripts de l'interface utilisateur Godot s'en servent pour désactiver les boutons d'achat lorsque le joueur n'a pas les fonds.

### Exemple de script d'interface (UI) :

```csharp
public partial class ShopMenuUI : Control
{
    private IShopManager m_shopManager;
    private IInventoryManager m_inventoryManager;

    [Export] private Button m_buyUpgradeButton;

    public override void _Ready()
    {
        // Récupérer les services du Core
        m_shopManager = ServiceRegistry.Get<IShopManager>();
        m_inventoryManager = ServiceRegistry.Get<IInventoryManager>();

        UpdateUI();
    }

    private void UpdateUI()
    {
        int upgradeLevel = 2; // Exemple: Niveau actuel
        int cost = m_shopManager.CalculateCost(upgradeLevel);

        // Le bouton est activé seulement si le joueur peut payer
        m_buyUpgradeButton.Disabled = !m_shopManager.CanAffordUpgrade(m_inventoryManager, "gold_01", cost);
        m_buyUpgradeButton.Text = $"Upgrade (Cost: {cost} Gold)";
    }
}
```

En gardant la logique de coût (`CalculateCost`) dans le `Core`, toute modification de l'équilibrage de l'économie se fera dans une classe testable unitairement, sans toucher aux scripts d'interface Godot.