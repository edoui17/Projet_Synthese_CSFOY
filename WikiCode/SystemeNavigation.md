# Documentation du Système de Navigation

Ce document décrit l'architecture et le fonctionnement "End-to-End" du système de navigation dans *IslandSurvivor*. Le système respecte une architecture N-Tier stricte, séparant la logique métier (Core) du moteur de jeu (Godot), et utilise le pattern de Bridge avec des `WeakEvent` pour la communication inter-systèmes.

## 1. Vue d'ensemble du Flux (End-to-End)

La navigation entre les îles se déroule en plusieurs étapes :

1. **Génération et Sélection :** Le joueur interagit avec un planificateur qui affiche le menu de navigation (`NavigationMenu`). Le `NavigationService` (Core) génère les destinations possibles et leurs coûts en ressources.
2. **Paiement et Validation :** Le joueur sélectionne une destination. Le `NavigationService` vérifie si l'inventaire (`IInventoryManager`) contient les ressources requises et les déduit le cas échéant.
3. **Activation du Portail :** Si la validation réussit, le `Portal` est activé dans la scène et se voit attribuer la destination choisie.
4. **Interaction et Événement :** Le joueur interagit avec le `PortalInteraction`. Cela déclenche l'émission de l'événement de navigation via le gestionnaire de signaux.
5. **Sauvegarde d'État :** Le `NavigationManager` (Godot) intercepte l'événement de navigation, sauvegarde l'inventaire et l'état de la session (comme `CurrentIslandId` dans `SessionState`). Cette sauvegarde garantit la persistance des données indépendamment du cycle de vie des scènes Godot.
6. **Transition de Scène :** Le `NavigationManager` délègue le changement effectif de scène au `SceneLoadingManager`.

---

## 2. Architecture N-Tier et Pattern Bridge

Le système est conçu pour isoler la logique pure de C# (Core) du moteur Godot, ce qui facilite les tests et la maintenance.

### 2.1 Le Service Core (`NavigationService`)

Implémenté dans `Src/Core/Managers/Navigation/NavigationService.cs`, ce service est responsable de :
- La génération procédurale des destinations possibles, basées sur des biomes et de la difficulté.
- L'évaluation du coût du voyage (`TryNavigate`).

L'avantage est que cette logique est découplée de toute interface utilisateur ou nœud Godot. Elle n'interagit qu'avec des interfaces C# abstraites (comme `IInventoryManager`).

### 2.2 Bridge Pattern et WeakEvents

Pour communiquer du Core vers Godot sans créer de fuites de mémoire (Memory Leaks), nous utilisons une implémentation personnalisée de `WeakEvent`.

Les nœuds Godot ne s'abonnent pas aux événements C# standards (`+=`, `-=`). Ils utilisent les méthodes d'extension :
- `.AddListener()`
- `.RemoveListener()`

Le point central de cette communication est le `SignalManager` (qui possède une contrepartie Core `SignalManagerCore`).

---

## 3. Détails d'Implémentation et Exemples de Code

### A. Sélection de destination et paiement

Le menu de navigation appelle le service du Core pour valider et déduire le coût du voyage :

```csharp
// Extrait de NavigationMenu.cs
bool success = m_navigationService.TryNavigate(InventoryNode.Instance.Manager, p_destination);

if (success)
{
    // Activation visuelle du portail avec la destination assignée
    var portalNode = GetTree().CurrentScene.GetNodeOrNull<Portal>("Portal");
    portalNode?.ActivatePortal(p_destination);
}
```

### B. Déclenchement de l'interaction (PortalInteraction)

L'interaction avec le portail est un exemple parfait de l'interface `IInteractable`. C'est le joueur qui décide du moment du départ, en interagissant explicitement avec le portail actif.

```csharp
// Extrait de PortalInteraction.cs
public void Interact()
{
    if (m_destination != null)
    {
        GD.Print($"[PortalInteraction] Emitting NavigationRequested for destination: {m_destination.Biome}");
        // Émission de l'événement vers le système global
        SignalManager.Instance.EmitNavigationRequested(this, m_destination);
    }
}
```

### C. Réception de l'événement, Sauvegarde et Transition

Le `NavigationManager` en Godot écoute `OnNavigationRequested`. Avant de changer la scène, il s'assure que tout l'état de jeu est persisté (inventaire et SessionState).

```csharp
// Extrait de NavigationManager.cs
public override void _Ready()
{
    // Utilisation du signal natif Godot (Bridge Pattern)
    SignalManager.Instance.NavigationRequested += OnNavigationRequested;
}

private void OnNavigationRequested(string p_islandId, string p_scenePath, string p_biome, int p_difficulty, int p_resourceCost, int p_dangerLevel)
{
    // 1. Sauvegarde de l'inventaire via GodotSaveService
    var saveService = new GodotSaveService();
    string inventoryJson = JsonSerializer.Serialize(InventoryNode.Instance.Manager.GetAllSlots());
    saveService.SaveData("inventory_save.json", inventoryJson);

    // 2. Mise à jour et sauvegarde de la session via le ServiceRegistry
    var tracker = ServiceRegistry.Instance.ScoreTracker;
    if (tracker != null)
    {
        tracker.UpdateCurrentIsland(p_islandId);
        string sessionJson = JsonSerializer.Serialize(tracker.GetSessionState());
        saveService.SaveData("session_save.json", sessionJson);
    }

    // 3. Délégation au SceneLoadingManager
    var slm = GetNodeOrNull<Managers.SceneLoadingManager>("/root/SceneLoadingManager");
    if (slm != null)
    {
        slm.LoadScene(p_scenePath);
    }
}
```

### D. Le SceneLoadingManager

Le `SceneLoadingManager` se charge d'effectuer la transition de manière sécurisée en utilisant `CallDeferred` pour éviter de changer d'arbre de scène au milieu du traitement de la physique ou d'un signal en cours.

```csharp
// Extrait de SceneLoadingManager.cs
public void LoadScene(string scenePath)
{
    // CallDeferred garantit que le moteur a fini le frame courant avant de charger la scène
    CallDeferred(nameof(ChangeScene), scenePath);
}

private void ChangeScene(string scenePath)
{
    var error = GetTree().ChangeSceneToFile(scenePath);
    if (error != Error.Ok)
    {
        GD.PrintErr($"[SceneLoadingManager] Failed to load scene {scenePath}. Error: {error}");
    }
}
```

---

## 4. Points Clés à Retenir

* **`SessionState`** : L'état dynamique mutable du joueur (score, île actuelle) est géré dans le `SessionState`. Cette approche complète la nature immuable des ressources Godot et ne dépend pas des nœuds du cycle de vie de la scène.
* **Coût Zéro "Test Island"** : Le premier élément de navigation (index 0) aura un coût forcé de zéro. Il s'agit d'un choix de Game Design et de développement (Test Island) qu'il ne faut pas traiter comme un bug.
* **Home Island** : La destination `HomeIsland` est persistée en tant que `IslandDestination.HomeIsland` (id: `home_hub`). S'y rendre est toujours gratuit (sans déduction de ressources).
* **Nettoyage des Events** : Le `NavigationManager` doit se désabonner des événements dans `_ExitTree()` avec `RemoveListener()` pour prévenir toute fuite de mémoire ou appel d'objets détruits.