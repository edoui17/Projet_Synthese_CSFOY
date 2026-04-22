# Forge's Journal - IslandSurvivor Technical Learnings

Ce document centralise les décisions architecturales, les particularités de Godot 4.6.1 et les patrons de synchronisation N-Tier pour le projet IslandSurvivor.

---

## 1. Architecture Globale & Injection (N-Tier)

- **Structure :** Séparation stricte entre **Core** (.NET 8 agnostique, `System.Numerics`) et **Client** (Godot 4.6.1).
- **ServiceRegistry (Autoload DI) :** Instancie l'**EventBus** (`Core.Services.EventBus`) et les services lors du `_EnterTree()`.
    - **Injection :** L'instance est passée aux Managers du Core via l'interface `IEventBus` par injection de constructeur.
    - **PubSub & File d'attente :** L'implémentation utilise une file d'attente différée. L'Autoload **doit** appeler `EventBus.ProcessEvents()` dans son `_Process(double delta)` pour drainer la file à chaque frame.
    - **Namespace :** Utiliser `global::Core.Services.EventBus` pour éviter les collisions MSBuild avec les projets de tests unitaires.

## 2. Conventions de Codage & Naming

- **Nomenclature (Standardisation) :**
    - **Manager :** Pour les systèmes globaux et services (ex: `SignalManager`).
    - **Controller :** Pour la logique spécifique attachée aux nœuds Godot (ex: `SheepController`).
    - **Paramètres :** Préfixer par `p_` dans les `delegate` de signaux Godot.
    - **Champs privés :** Commencer par `m_`.
- **Typage :** Mot-clé `var` strictement **interdit**. Types explicites uniquement.
- **Isolation des Classes :** Une classe par fichier. 
    - *Exception d'atomicité* : Les classes privées imbriquées (ex: `WeakDelegate` dans `WeakEvent`) restent groupées pour l'encapsulation.
- **Ordre des membres :** 1. Champs privés (`m_`) | 2. Constructeurs | 3. Propriétés | 4. Méthodes.

## 3. Communication : Bridge Pattern & Événements

- **Core (WeakEvent) :** Utilise `WeakReference`. Inscription via `.AddListener()` et désinscription via `.RemoveListener()` (les opérateurs `+=` / `-=` sont proscrits).
- **Bridge Godot (SignalManager) :** Le Bridge (Autoload) intercepte les `WeakEvents` et les ré-émet via des `[Signal]`.
- **Décomposition des Types :** Les signaux Godot ne supportent pas les classes C# pures. Le Bridge doit décomposer les objets complexes (ex: `IslandDestination`) en types primitifs (`string`, `int`) lors de la ré-émission.

## 4. Systèmes de Jeu & Physique (Client Godot)

- **Audit de Domaine :** Les entités liées à la boucle visuelle/physique (`SheepController`, `HealthComponent`) résident dans le Client (`Logic/Entities/`). Le Core reste pur pour être partageable avec une API.
- **Mouvement (US 5.4) :** Le `MovementController` utilise `MoveAndSlide()`. L'usage de la physique native Godot est prioritaire sur un solveur personnalisé dans le Core pour la performance.
- **Physique :** **Layer** = Identité du nœud | **Mask** = Ce que le nœud détecte.
- **Nettoyage :** Appeler `SignalManager.Instance.EmitMaterialDestroyed(...)` puis `QueueFree()` pour garantir la libération mémoire.

## 5. UI et Interaction (CanvasLayer)

- **CanvasLayer :** Les menus globaux (`NavigationMenu`) doivent y être placés pour rester au premier plan du viewport et ne pas être affectés par la caméra.
- **Input Absorption :** Évite que les collisions ou filtres de souris des `Node2D` n'absorbent les événements destinés à l'UI.
- **Synchronisation :** L'état de l'UI (ex: bouton grisé) est piloté par la logique du Core relayée par le Bridge.

## 6. Navigation & Gestion de Scènes

- **Validation :** Le `NavigationService` (Core) effectue une validation synchrone des ressources (via `IShopManager`/`IInventoryManager`) avant de dispatcher un `NavigationRequestedEvent`.
- **Portails Auto-Actifs :** Les portails hors du `PlayerHub` s'activent au `_Ready()` via `GetTree().CurrentScene.SceneFilePath` pour servir de point de retour permanent.
- **Injection de Joueur :** Le joueur est extrait des maps de base pour être placé directement dans les scènes `Level{X}.tscn`.
- **Héritage de Scènes :** Requiert un index pathing spécifique dans le `.tscn` enfant pour injecter des éléments dans les conteneurs hérités (ex: `MapContainer`).

## 7. Persistance & Base de Données (DBIslandSurvivor)

- **Infrastructure EF Core :** Domain models dans `Src/Core/Domain` et mappage Fluent API dans `Src/Infrastructure`.
- **Schéma SQL :** - Clés primaires en **GUID** (`uniqueidentifier`).
    - Longueurs de string IDs fixes pour correspondre au schéma SQL.
    - **ExtraStats (NVARCHAR(MAX)) :** Colonne JSON pour la flexibilité sans migration.
    - Champs exclus (non-essentiels API) : `Resolution`, `IsFullScreen`.
- **Seeding (data.sql) :** Vitesse de base fixée à **1**. Utilisation de variables T-SQL (`@ForgeId`) pour garantir l'intégrité référentielle entre les tables `Stats`, `Inventory` et `PlayerConfig`.
- **File System :** Utiliser `ProjectSettings.GlobalizePath("res://../../Save/")` avec `DirAccess`/`FileAccess` pour gérer les sauvegardes hors du dossier `res://`.

## 8. Maintenance & Quirks Techniques

- **Corruption Git :** Les marqueurs de conflit (`<<<<<<< HEAD`) corrompent le parsing des fichiers `.tres` et `.tscn`. Correction manuelle via éditeur de texte requise.
- **Dépendances .NET :** En cas de migration de `System.Numerics.Vector2` (Core) vers le Client, privilégier `Godot.Vector2` pour la cohésion avec les APIs natives (`Normalized()`, `MoveAndSlide()`).