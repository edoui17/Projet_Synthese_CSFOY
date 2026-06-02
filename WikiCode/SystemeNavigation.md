# Documentation du Système de Navigation

Ce document décrit l'architecture et le fonctionnement "End-to-End" du système de navigation dans *IslandSurvivor*. Le système respecte une architecture N-Tier stricte, séparant la logique métier (Core) du moteur de jeu (Godot), et utilise le pattern de Bridge et l'**EventBus** pour la communication inter-systèmes.

## 1. Vue d'ensemble du Flux (End-to-End)

La navigation entre les îles se déroule en plusieurs étapes :

1. **Génération et Sélection :** Le joueur interagit avec un planificateur qui affiche le menu de navigation (`NavigationMenu`). Le `NavigationService` (Core) génère les destinations possibles et leurs coûts en ressources. L'UI affiche désormais de façon détaillée les coûts en ressources directement sur les boutons de la boutique d'îles.
2. **Paiement et Validation :** Le joueur sélectionne une destination. Le `NavigationService` vérifie si l'inventaire (`IInventoryManager`) contient les ressources requises et les déduit le cas échéant.
3. **Activation du Portail :** Si la validation réussit, le `Portal` est activé dans la scène et se voit attribuer la destination choisie. Le label d'interaction "Press E to Interact" s'affiche correctement à l'aide d'un `CanvasLayer` pour se superposer proprement par-dessus l'environnement.
4. **Interaction et Événement :** Le joueur interagit avec le `PortalInteraction`. Cela publie un événement `NavigationRequestedEvent` sur l'EventBus.
5. **Sauvegarde d'État :** L'Autoload `NavigationManager` (Godot) écoute cet événement, sauvegarde l'inventaire et l'état de la session (comme `CurrentIslandId` dans `SessionState`). Cette sauvegarde garantit la persistance des données indépendamment du cycle de vie des scènes Godot.
6. **Transition de Scène :** Le `NavigationManager` délègue le changement effectif de scène au `SceneLoadingManager`. Ce dernier affiche un écran de chargement et force explicitement le moteur à faire un rendu (pendant 2 frames) avant de démarrer des opérations potentiellement bloquantes.

---

## 2. Architecture N-Tier et Pattern Bridge

Le système est conçu pour isoler la logique pure de C# (Core) du moteur Godot, ce qui facilite les tests et la maintenance.

### 2.1 Le Service Core (`NavigationService`)

Implémenté dans `Src/Core/Managers/Navigation/NavigationService.cs`, ce service est responsable de :
- La génération procédurale des destinations possibles, basées sur des biomes et de la difficulté.
- L'évaluation du coût du voyage (`TryNavigate`).

L'avantage est que cette logique est découplée de toute interface utilisateur ou nœud Godot. Elle n'interagit qu'avec des interfaces C# abstraites (comme `IInventoryManager`).

### 2.2 Communication via l'EventBus

Pour communiquer du Core vers Godot sans créer de fuites de mémoire et en respectant l'isolation stricte, le système utilise l'**EventBus**.

- Le Core publie des événements (ex: `NavigationApprovedEvent` ou `NavigationRejectedEvent`).
- Le `SignalManager` (qui agit comme Translator/Bridge en Godot) ou le `NavigationManager` s'abonnent à ces `IEvent`s et gèrent la logique propre au moteur Godot (effets visuels, chargement de scène).

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
        GD.Print($"[PortalInteraction] Emitting NavigationRequestedEvent for destination: {m_destination.Biome}");
        // Émission de l'événement sur l'EventBus
        ServiceRegistry.Instance.EventBus.Publish(new NavigationRequestedEvent(m_destination));
    }
}
```

### C. Réception de l'événement, Sauvegarde et Transition

Le `NavigationManager` en Godot (Autoload) s'abonne au `NavigationRequestedEvent`. Avant de changer la scène, il s'assure que tout l'état de jeu est persisté (inventaire et SessionState).

```csharp
// Extrait de NavigationManager.cs
public override void _Ready()
{
    // Abonnement au SignalManager pour écouter les demandes de téléportation
    SignalManager.Instance.TeleportRequested += OnTeleportRequested;
}

private async void OnTeleportRequested(string p_islandId, string p_scenePath, string p_biome, int p_difficulty, int p_resourceCost, int p_dangerLevel)
{
    // 1. Affiche l'écran de chargement
    var slm = GetNodeOrNull<Managers.SceneLoadingManager>("/root/SceneLoadingManager");
    if (slm != null)
    {
        slm.ShowLoading();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    // 2. Construit la requête de synchronisation avec l'inventaire actuel
    SyncRequest syncRequest = new SyncRequest();
    // ... (mapping de InventoryNode.Instance.Manager.GetAllSlots() vers syncRequest.Inventory)

    // 3. Met à jour l'île actuelle dans le tracker de score
    IScoreTracker tracker = ServiceRegistry.Instance.ScoreTracker;
    if (tracker != null)
    {
        tracker.UpdateCurrentIsland(p_islandId);
    }

    // 4. Lance la tâche de synchronisation via ApiService
    bool success = await ServiceRegistry.Instance.ApiService.SyncAsync(syncRequest);

    // 5. Délégation au SceneLoadingManager pour changer la scène
    if (slm != null)
    {
        slm.LoadScene(p_scenePath);
    }
}
```

### D. Le SceneLoadingManager (Écran de Chargement)

Le `SceneLoadingManager` se charge d'effectuer la transition de manière sécurisée en affichant d'abord un écran de chargement. Pour garantir que l'interface de chargement s'affiche effectivement avant toute opération lourde (comme le chargement synchrone d'une nouvelle scène ou une requête réseau), le gestionnaire force explicitement un rendu pendant 2 frames avec `await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame)`.

```csharp
// Extrait conceptuel de SceneLoadingManager.cs
public async void LoadScene(string scenePath)
{
    // Affiche l'écran de chargement
    m_loadingScreen.Show();

    // Force Godot à dessiner l'UI avant de bloquer le thread principal
    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

    CallDeferred(nameof(ChangeScene), scenePath);
}

private void ChangeScene(string scenePath)
{
    var error = GetTree().ChangeSceneToFile(scenePath);
    if (error != Error.Ok)
    {
        GD.PrintErr($"[SceneLoadingManager] Failed to load scene {scenePath}. Error: {error}");
    }
    // L'écran de chargement est masqué une fois la nouvelle scène prête.
}
```

---

## 4. Points Clés à Retenir

* **`SessionState`** : L'état dynamique mutable du joueur (score, île actuelle) est géré dans le `SessionState`. Cette approche complète la nature immuable des ressources Godot et ne dépend pas des nœuds du cycle de vie de la scène.
* **Coût Zéro "Test Island"** : Le premier élément de navigation (index 0) aura un coût forcé de zéro. Il s'agit d'un choix de Game Design et de développement (Test Island) qu'il ne faut pas traiter comme un bug.
* **Home Island** : La destination `HomeIsland` est persistée en tant que `IslandDestination.HomeIsland` (id: `home_hub`). S'y rendre est toujours gratuit (sans déduction de ressources).
* **Nettoyage des Events** : Même avec l'EventBus, il est recommandé aux nœuds Godot non-globaux (ex: une UI) de se désabonner des événements dans `_ExitTree()` avec `Unsubscribe<T>()` pour prévenir toute fuite de mémoire ou appel d'objets détruits. L'Autoload `NavigationManager`, persistant durant toute la durée de vie du jeu, le gère de façon globale.