# IslandSurvivor - Journal d'Utilisation de l'IA (KH)

Ce document retrace l'interaction entre Kevin Houle et les assistants IA (Forge/Atlas) pour documenter le processus de conception et le rôle de l'IA dans le projet.

---

## Historique d'Utilisation

### 2026-03-29 - Configuration de l'Assistant "Forge"
- **Requête :** Créer un prompt système complet pour un assistant IA (Forge) afin d'aider au développement du projet Roguelike "IslandSurvivor".
- **Contribution de l'IA :** Définition des frontières architecturales (N-Tier), des conventions de codage (préfixe `m_` pour les champs et `p_` pour les paramètres) et établissement d'un processus de journalisation rigoureux.
- **Raisonnement :** Choix d'une approche basée sur une Persona (Forge) pour garantir la cohérence du style de code et l'intégrité architecturale, en séparant la logique partagée (Core) de la présentation (Godot/Blazor).

### 2026-03-29 - [User Story 5.3] Gestionnaire de signaux global (SignalManager)
- **Requête :** Implémenter un SignalManager global (Autoload) pour assurer une communication fluide entre les systèmes de jeu sans fuites de mémoire.
- **Contribution de l'IA :** Conception et implémentation d'un pattern `WeakEvent` en C# (Core) utilisant `WeakReference` pour permettre au Garbage Collector de nettoyer automatiquement les nœuds Godot détruits. Création de `ISignalManager` et de ses implémentations. Écriture de tests xUnit. Refactorisation pour séparation stricte des fichiers et ordre des membres.
- **Raisonnement :** Choix d'événements C# purs avec `WeakReference` pour découpler la logique `Core` du moteur Godot et permettre des tests unitaires complets sans environnement Godot.

### 2026-03-30 - [User Story 5.1] Implémenter le gestionnaire de statistiques
- **Requête :** Implémenter le gestionnaire de statistiques d'entité (Santé, Attaque, Chance, Vitesse) en suivant l'architecture N-Tier.
- **Contribution de l'IA :** Construction d'un tracker de stats basé sur des Enum dans `/Src/Core`. Implémentation des classes `StatTracker` et `Stat`. Dans Godot, exposition d'une config `[GlobalClass]` via `EntityStats` (Resource), chargée par un nœud `StatManager` via le pattern Bridge, réémettant les signaux `WeakEvent` vers des `[Signal]` natifs de Godot.
- **Raisonnement :** L'approche par "Pont" (Bridge) permet au Core de rester ignorant de Godot tout en restaurant les fonctionnalités de l'éditeur Godot.

### 2026-04-09 - [US 3.2 : Implémenter la navigation entre les îles]
- **Requête :** Ajouter un système de navigation, suivre l'île actuelle, gérer les coûts de transition et la persistance.
- **Contribution de l'IA :** 1. **Core :** Ajout de `IslandDestination`, extension de `SessionState`. Logique de déduction de ressources dans `NavigationService`.
    2. **Godot Bridge :** Extension de `SignalManager`. 
    3. **UI/Interaction :** Création de `NavigationMenu.tscn` et `PortalInteraction.cs`.
    4. **Intégration :** `NavigationManager` (autoload) gère le changement de scène et la sérialisation via `ISaveService` avant la transition.
- **Raisonnement :** Le joueur interagit avec un portail physique pour déclencher la transition, gardant le gameplay immersif. L'interception de la transition dans le `NavigationManager` assure que l'inventaire et l'état de la session sont sauvegardés sur le disque.

### 2026-04-11 - [Feature: Gestion des Layers de Collision]
- **Requête :** Configurer les layers de collision pour corriger l'absence d'interaction entre le Player et le building_node.
- **Contribution de l'IA :** - Configuration des layers 2D : Environnement, Interaction, Player, Combat, Ressource.
    - Correction des masques de collision dans `Player.tscn` et `building_node.tscn`.
    - Mise à jour des ressources (Gold, Rock, Portal, etc.).
    - Documentation dans `WikiCode/CollisionLayers.md`.
- **Raisonnement :** La séparation des layers (ex: Interaction vs Corps physique) optimise la détection physique et évite les faux positifs ou les conflits entre la logique de combat et de terrain.

### [User Story 5.2 : Implémenter le système de point]
- **Requête :** Accumulation du score, validation des valeurs positives, signal de mise à jour UI et persistance du High Score.
- **Contribution de l'IA :** 1. **Core :** `SessionState` pour les données mutables de runtime. Création de `ISaveService` (Inversion de dépendance). 
    2. **Managers :** `ScoreTracker` gérant la logique de validation et de persistance.
    3. **Godot :** `ScoreManager` agissant comme pont entre le Core et l'UI Godot. Tests unitaires avec Moq.
- **Raisonnement :** Isolation de l'état "Live" pour éviter de muter les ressources Godot (templates) au runtime, simplifiant ainsi la sérialisation JSON.

### [US 3.1 : Génération Procédurale (Map, Elévation, Splash)]
- **Requête :** Implémenter la génération d'îles (Plateaux, Falaises, Escaliers) et les transitions visuelles (écume).
- **Contribution de l'IA :** - Création de `IMapGenerator` dans le Core. Implémentation de `GodotIslandGenerator` avec `FastNoiseLite` et algorithme BFS.
    - Logique de "multi-pass" pour l'élévation (bordures de falaises).
    - Ajout automatique de tuiles d'écume (`FoamWaterTileMap`) par vérification de voisinage dans Godot.
    - Création du `MapRenderer.cs` pour traduire les données du Core en `SetCellsTerrainConnect`.
- **Raisonnement :** La logique visuelle (écume) reste dans Godot, tandis que la structure de la map est générée par le Core sous forme de données neutres (strings), respectant la séparation N-Tier.

### [Bugfix: NavigationMenu non visible]
- **Requête :** Le menu de navigation n'apparaît pas lors de l'interaction.
- **Contribution de l'IA :** Modification de `MaterialsMenuPlanner.cs` pour utiliser un chemin de nœud robuste (`GetTree().Root...`) et appel de `OpenMenu()` au lieu de simplement changer la visibilité.
- **Raisonnement :** Forcer uniquement `Visible = true` ne déclenchait pas la génération dynamique des boutons. L'appel explicite à la méthode dédiée garantit l'initialisation de l'UI.

### 2026-04-11 - [UI / Interaction] Refactor BuildingNode to UI Signal Logic
**Request**: Implement a good signal logic for `building_node` so it can properly interact with UI components, especially the Material Planner.
**AI Contribution**:
1. Added `BuildingShopToggledEventArgs` and a corresponding `WeakEvent` (`OnBuildingShopToggled`) in the `ISignalManager` and `SignalManagerCore` inside the Core domain.
2. Exposed `OnBuildingShopToggled` in the Godot-side `SignalManager` (Bridge pattern).
3. Updated `BuildingNode.cs` to emit `OnBuildingShopToggled` with `true` on interact, and `false` when the player exits the interaction area.
4. Updated `MaterialsMenuPlanner.cs` to subscribe to the new signal and toggle its visibility based on `e.IsOpen`.
**Decision Reasoning**: N-Tier architecture relies on a Bridge Pattern for cross-system communication. Passing these state changes through pure C# `WeakEvents` in the Core layer before re-emitting them in the Godot layer maintains decoupled systems. The UI reacts to global domain state rather than being tightly coupled to a specific physical node.

### 2026-04-11 - [UI / Interaction] Fix Menu Rendering and Clicks
**Request**: Assure toi que le menu material Planner est afficher a la meme posision que le building_node lorsqu'il est ouvert. Puis, assure toi que le navigation menu fonctionne bien, les bouton ne detecte pas d'input, probablement un probl'eme de layer ou de positionnement dans les scenes.
**AI Contribution**:
1. Moved `NavigationMenu` out of `base_map_island.tscn` (world space) into `main.tscn`'s `CanvasLayer` (screen space) so it properly receives input and renders above the game world.
2. Modified `building_node.tscn` to remove the `CanvasLayer` wrapping `GameMaterialControl`, attaching it directly to the root `Node2D`. This ensures the menu renders in world space precisely at the position of the building instead of locking to the camera viewport.
3. Updated `MaterialsMenuPlanner.cs` to query the new node path of `NavigationMenu` from `Main/CanvasLayer/NavigationMenu`.
**Decision Reasoning**: Global interactive UI elements like the main island navigation menu need to be in a `CanvasLayer` to correctly process OS-level mouse inputs without being blocked by 2D Game World elements (like Y-Sort objects or Area2Ds). Conversely, local UI meant to simulate an in-world popup for a specific object (like the Material Planner) should exist in world space (without `CanvasLayer`) so it maintains its physical position relative to the object it belongs to.
### 2024-04-12 - [Navigation Menu fixes]
| **Request** | Fix Navigation Menu return to home logic, portal behavior, and display format. | **AI Contribution** | Extracted `Player` out of `Map.tscn` to `Level.tscn`, implemented `PlayerHub.tscn`, added `SceneLoadingManager.cs` singleton, and updated `NavigationMenu.cs` to handle home checks using `SessionState`. | **Decision Reasoning** | Improved world shell hierarchy by moving `Player` to `Level{X}.tscn` scenes. Allows UI and Navigation to easily find Player using generic methods instead of hardcoded paths inside dynamically loaded maps.

### 2026-04-11 - [Navigation Menu] Force first destination to be free for testing
**Request**: Force the very first destination generated in the Navigation Menu to have a resource cost of 0 and label it as a "Test" island.
**AI Contribution**:
1. Modified `NavigationService.cs` so `GenerateDestinations` explicitly sets `resourceCost = 0` when `i == 0`.
2. Modified `NavigationMenu.cs` to dynamically prepend "Test " to the island button label if `destination.ResourceCost == 0`.
**Decision Reasoning**: Satisfies the requirement for a permanent free test destination without altering the core structure of the 5 generated islands. This maintains UI flexibility and keeps test overrides explicit.

### 2026-04-12 - [Gameplay / Navigation] Auto-Active Return Portal
| **Request** | The portal when I am not home should always be active to let me return home. | **AI Contribution** | Modified `Portal.cs` to check the `SceneFilePath` during `_Ready()`. If the path does not contain `PlayerHub`, it invokes `ActivatePortal(IslandDestination.HomeIsland)`. | **Decision Reasoning** | Since portals exist universally in the map templates, automatically triggering it upon loading a non-hub level guarantees a seamless return path without needing to tie portal activation to complex level logic or specific map events.

### 2026-04-12 - [Documentation] SystemeNavigation.md wiki page
| **Request** | J'aimerais que tu ajoute une page wiki pour documenter comment utiliser le systeme de navigation, incluant le SceneLoadingManager | **AI Contribution** | Analyzed the codebase to understand the Navigation flow (from UI interaction to the core NavigationService, down to SceneLoadingManager via WeakEvents). Drafted an exhaustive markdown documentation (`WikiCode/SystemeNavigation.md`) in French, demonstrating the N-Tier architecture, Bridge Pattern with WeakEvents, and providing concrete code examples of interaction triggers, Core service validation, and scene transitions. | **Decision Reasoning** | Following the user's explicit instruction to create exhaustive technical documentation that highlights the solid engineering (N-Tier, event handling, persistence decoupled from Godot lifecycle) behind the Navigation System, ensuring it serves as a robust reference for future development.

### 2026-04-14 - [Save Location Migration]
| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| Move Godot saves to root `Save` folder. | Updated `GodotSaveService.cs` to use `res://../../Save/` and implemented migration from `user://` to this new folder. | `res://../../` natively escapes the IslandSurvivor folder to target the .sln root. Adding dynamic `EnsureDirectoryExists()` and checking `.json` files enables automatic migration without data loss. |

### $(date +'%Y-%m-%d') - [Level Architecture Refactoring]
| **Request** | Extract common elements (Player, UI CanvasLayer, and ScoreManager) from various level scenes into a single inheritable `LevelBase.tscn` to simplify level design and support future Score tracking systems. |
| **AI Contribution** | Created `LevelBase.tscn` incorporating `MapContainer`, `Player`, `ScoreManager`, and UI nodes (`CanvasLayer` containing `PlayerHUD`, `NavigationMenu`, and `MaterialsMenuPlanner`). Then modified `Level1` through `Level5` and `PlayerHub` to inherit from `LevelBase.tscn`, specifically appending their respective map assets and individual portals to the inherited tree. |
| **Decision Reasoning** | Aligning with the N-Tier architecture principles, a shared Godot Base Scene allows unified inclusion of shared nodes like the `ScoreManager` without manually replicating them across scenes, ensuring maintainability and adherence to DRY principles. |

### 2024-04-16 - [Audit Phase 1 - Structure et Agnosticisme]
| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| Effectuer un audit et corriger la hiérarchie des fichiers, l'agnosticisme du Core vis-à-vis de Godot, et l'atomicité des classes (1 classe par fichier). | - Déplacement des entités spécifiques au jeu et de leurs interfaces (ex: `SheepController`, `HealthComponent`) de `/Src/Core/` vers `/Src/IslandSurvivor/`.<br>- Remplacement de `System.Numerics.Vector2` par `Godot.Vector2` dans le code propre au jeu.<br>- Extraction des sous-classes (`SheepStates`, `ScoreChangedEventArgs`) vers des fichiers séparés.<br>- Découpage des tests unitaires (`SheepLogicTests` divisé en deux fichiers). | - Les entités exclusivement liées au comportement du jeu ne seront jamais utilisées dans le Web ou l'API, elles appartiennent donc à `IslandSurvivor`.<br>- Lors de la migration vers le jeu, le choix natif `Godot.Vector2` est préférable à la librairie de base C#.<br>- Le maintien des délégués privés imbriqués (comme dans `WeakEvent`) respecte les bonnes pratiques d'encapsulation C# tout en adhérant globalement à l'atomicité. |

### $(date +"%Y-%m-%d") - [Audit Phase 2 - Injection et Bridge Pattern]
- **Request**: Audit the core communication layer (ServiceRegistry DI and SignalManager Bridge pattern) to ensure robust memory management and strict N-Tier compliance. Clean up obsolete Godot/Core code logic (like GameManager Map creation).
- **AI Contribution**: Created `ServiceRegistry` to act as the global DI container. Refactored `SignalManager` to act as a proper Bridge (absorbing `WeakEvents` from Core and re-emitting Godot `[Signal]`). Migrated all UI and Node listeners to use standard Godot C# event syntax (`+=`).
- **Decision Reasoning**: Using Godot `[Signal]` handles Godot's node lifecycle safely preventing Lapsed Listener leaks upon scene change. Implementing a pure DI `ServiceRegistry` fixes strong coupling and transient state issues.

### 2024-05-30 - [Audit Phase 3 - Ressources et Signaux]
| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| Audit des Nodes, SRP, paramètres de signaux, QueueFree, et Fix de conflits | Résolution de merge conflicts (GoldStats.tres, test_signal.tscn), renommage des paramètres avec le préfixe `p_` dans les scripts de IslandSurvivor (Signaux et Event handlers), vérification du QueueFree et validation des tres. | Résoudre les conflits Git était primordial car ils créaient des balises invalides empêchant le moteur Godot de charger les ressources. L'audit de SRP a confirmé que Session et Entity Stats devaient rester séparés. Le refactoring des variables respecte la convention stricte p_ pour harmoniser le code. |

### 2026-04-17 - [Audit Phase 4 - Documentation et Clôture]
- **Request**: Mettre à jour les fichiers du Wiki pour qu'ils reflètent les derniers changements architecturaux de la phase d'audit (Bridge Pattern natif, Dependency Injection via ServiceRegistry) et sceller l'itération 0.1 pour servir de référence.
- **AI Contribution**:
  1. `WikiCode/SignalManager.md` réécrit pour indiquer l'utilisation exclusive du Bridge (signaux natifs `+=`) du côté Client Godot.
  2. `WikiCode/Systeme_Inventaire.md` mis à jour pour indiquer que l'instance de `InventoryManager` provient du `ServiceRegistry` et non d'une instanciation manuelle dans `_Ready()`.
  3. `WikiCode/ScoreManager.md` mis à jour pour spécifier l'injection de dépendances pour le `ScoreTracker`.
  4. `WikiCode/SystemeNavigation.md` corrigé avec la syntaxe de signal `+=` et l'utilisation de `ServiceRegistry.Instance.ScoreTracker`.
  5. Finalisation du document `WikiCode/Audit/Iteration1_Audit.md` en y ajoutant la section de clôture de la Phase 4.
- **Decision Reasoning**: La documentation doit être strictement "User Oriented". Les exemples précédents montraient encore l'utilisation directe de `WeakEvent` (`.AddListener`) et d'instanciation de Managers (`new Manager()`), ce qui contredisait le nouvel Audit de Phase 2. Les mises à jour s'assurent que les développeurs comprennent comment s'abonner correctement via Godot pour éviter les fuites de mémoire sans enfreindre la séparation N-Tier.

### 2026-04-17 - [Audit Phase 4 - Wiki Maintenance & Cleanup]
- **Request**: Effectuer une revue de tous les fichiers .md dans le Wiki pour s'assurer qu'ils reflètent l'état actuel du code (identifier les lacunes ou l'obsolescence) et créer de nouvelles pages si des systèmes manquent. Ne pas supprimer Map.md.
- **AI Contribution**:
  1. Suppression de `ProceduralMapGeneration.md` car le système de génération procédurale a été abandonné. Conservation de `Map.md`.
  2. Suppression de `Inventory.md` (fichier vide en double).
  3. Création de `Architecture_Niveaux.md` pour documenter l'utilisation de l'héritage de scène (`LevelBase.tscn`), l'injection de `MapContainer`, et le positionnement UI via `CanvasLayer`.
  4. Mise à jour de `CollisionLayers.md` pour clarifier mathématiquement le système de Bitmask (puissance de 2) lié aux IDs des couches Godot.
- **Decision Reasoning**: La suppression des fichiers obsolètes allège la charge mentale. La conservation de Map.md satisfait les règles établies. La documentation de la nouvelle architecture des niveaux (qui n'était expliquée nulle part de manière centralisée) donne à l'équipe une source de vérité claire et saine pour la suite du développement.

### 2024-04-20 - [US 5.4 Créer une composante de mouvement unifié]
| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| Implement a reusable MovementController for Player and NPCs (Sheep). The system needs to use a base speed and apply the Speed stat as a percentage increase. Keep the logic in the game project instead of Core. | Implemented `MovementController.cs` and integrated it into `Player.tscn` / `Sheep.tscn`. Set up stat multiplication so that `Speed` stat gives +1% per point. Wrote the system documentation. | Kept the physics inside Godot since `MoveAndSlide` and Godot's internal physics loop are much better suited for 2D character collision and slope handling. The user confirmed keeping this out of `Core` to avoid duplicating physics code. |

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| Fix `CS1503`, `CS7036`, and `CS1501` compilation errors caused by new `IEventBus` constructor requirements and `TryNavigate` signature change. | Implemented `EventBus` instantiation in `ServiceRegistry`, injected it into Core managers, and set up `_Process` for deferred event processing. Refactored `NavigationMenu.cs` to fetch `NavigationService` from `ServiceRegistry.Instance` and updated `TryNavigate` calls. Fixed all corresponding unit tests. | Centralizing infrastructure instantiation in `ServiceRegistry` respects N-Tier architecture and DI principles. Overriding `_Process` guarantees deferred events execute correctly in the Godot lifecycle. Removing `InventoryManager` from `TryNavigate` cleanly decouples navigation logic from inventory validation, delegating communication to the EventBus. |
### 2026-04-22 - [Navigation System Fix]
**Request**: Fix the navigation system so that islands cannot be purchased without sufficient resources, ensuring N-Tier separation.
**AI Contribution**: Implemented synchronous resource checking in `NavigationService` and `NavigationMenu` by injecting `ShopManager` and `InventoryManager` into `NavigationService`. Updated the Godot UI to disable buttons dynamically, and updated unit tests.
**Decision Reasoning**: The `NavigationMenu` now correctly queries `NavigationService.CanAffordIsland()` to keep UI decoupled from shop logic. The asynchronous transaction processing is maintained in `InventoryManager` via EventBus, but now the initial request is securely blocked if resources are inadequate.
### 2026-04-22 - [Navigation Menu UI Revert]
**Request**: Remove the text modifications and disabling of UI buttons in the Navigation Menu when the player cannot afford an island. Prefer using GD.Print logs and keeping UI active.
**AI Contribution**: Reverted visual alterations inside `NavigationMenu.cs` so that the button strictly displays the destination data.
**Decision Reasoning**: Retained the core functionality of blocking the backend purchase via the synchronous check in `NavigationService`, but restored the UI text so it doesn't clutter the user interface.

### 2026-04-23 - [Système] Bridge Architecture Refactor
**Request**: Implement the pub-sub architecture strictly. Refactor the SignalManager to act as a bridge between Godot Signals and the EventBus, ensuring Core managers no longer use WeakEvents directly.
**AI Contribution**: Analyzed the project architecture. Deleted obsolete `ISignalManager` and `SignalManagerCore`. Created required `IEvent`s. Migrated `ScoreTracker`, `StatTracker`, and `NavigationService` to rely strictly on the `IEventBus`. Refactored Godot `SignalManager` to translate native Godot signals into `IEvent`s and vice-versa. Updated Global Godot Nodes (`ScoreManager`, `StatManager`) to directly subscribe to the `EventBus` for updates. Updated Unit tests to match the new dependencies.
**Decision Reasoning**: In N-Tier architecture, the Core layer must remain completely agnostic of engine-specific concepts (like Godot Signals). Providing an event bus ensures loose coupling, and the Godot-side SignalManager translates interactions cleanly without bleeding engine paradigms into Core business logic.

### $(date +"%Y-%m-%d") - [US 8.1 Data Persistence via API & DB]
- **Request**: Implement the data synchronization logic to communicate with the existing database schema and API, redirecting the local save logic to act as a fallback cache.
- **AI Contribution**:
  1. Removed default WeatherForecast endpoint from `API/Program.cs` and enabled standard controllers mapping.
  2. Implemented `IApiService` and `ApiService` for HTTP `LoginAsync`, `GetProfileAsync`, and `SyncAsync`.
  3. Integrated `ApiService` into Godot's Autoload `ServiceRegistry` to fetch and publish `ProfileLoadedEvent` upon startup.
  4. Updated `NavigationManager` to construct a `SyncRequest` from the core managers and post it via `ApiService.SyncAsync` upon every scene transition.
- **Decision Reasoning**: Preserving `ISaveService` inside `ApiService` satisfies the offline fallback requirement. Executing sync calls during scene transitions leverages the existing, safe synchronization barrier (`NavigationManager` handling deferred loads). By generating new `IEvent` payloads (`SyncStartedEvent`, etc.), the architecture strictly honors the Pub-Sub boundaries instead of introducing coupling.
