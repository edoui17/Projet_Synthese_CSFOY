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

### Gestion des Sauvegardes
- **Chemins de fichiers :** Pour sortir de `res://` vers la racine de la solution (`/Save/`), utiliser `ProjectSettings.GlobalizePath("res://../../Save/")`.
- **API :** Prioriser `DirAccess` et `FileAccess` de Godot pour respecter les répertoires virtuels (`user://`).

---

## 6. Maintenance & Quirks Techniques
- **Audit Phase 3 (Ressources) :** - Les marqueurs de conflits Git (`<<<<<<< HEAD`) corrompent le parsing des fichiers `.tres` et `.tscn`. Correction manuelle requise.
    - Préfixer les paramètres des `delegate` de signaux par `p_` pour la conformité.
- **Physique :** - **Layer** = Ce que je suis (Player: Layer 3).
    - **Mask** = Ce que je détecte (Interactibles: Mask 2).

### 2026-04-11 - WeakEvent Subscription in Core Bridge
- **Discovery**: Custom `WeakEvent` implementation in the Core uses `.AddListener()` and `.RemoveListener()` instead of the standard `+=` and `-=` operators or `.Subscribe()` / `.Unsubscribe()`. Always verify the specific custom utilities provided by the Core before trying to attach standard C# event handler logic.

### 2026-04-11 - UI Placement in Godot 4.6.1
- **Discovery**: When placing `Control` nodes directly inside a `Node2D` hierarchy (world space), they correctly follow the world coordinates and are affected by the camera. Wrapping them in a `CanvasLayer` detaches them from world coordinates and pins them to screen space.
- **Quirk**: Input events for UI elements (like `Button` clicks) can be absorbed or ignored if the UI is deep in the game world tree and obscured by collision/mouse filters of sibling `Node2D`/`Area2D`s. Placing global interactive menus (like `NavigationMenu`) into the main scene's `CanvasLayer` guarantees they are at the forefront of the viewport and reliably intercept mouse events.

### Navigation Menu Fixes & Level Architecture
- Extracted the Player from base maps and put them directly in `Level{X}.tscn` scenes.
- Created `PlayerHub.tscn` to load `base_map_island.tscn` cleanly.
- `NavigationManager` now uses `SceneLoadingManager` for loading.
- Re-routed all portal interactions to spawn portals near player or just remain static at the Home location, updating `IsPlayerHome` check to check `SessionState`.
### 2026-04-12 - Auto-Active Portal for Map Return
- **Feature**: Portals in any island maps that are not the `PlayerHub` will now be automatically activated.
- **Discovery**: Godot allows retrieving `GetTree()?.CurrentScene?.SceneFilePath` during `_Ready()` of any node inside the active scene. We leverage this to conditionally invoke `ActivatePortal(IslandDestination.HomeIsland)` right when the portal loads, allowing the portal to permanently serve as a return point back to the home hub.

### 2026-04-12 - Navigation System Architecture Documentation
- **Tags**: Navigation, Système, Architecture
- **Decision**: Created an exhaustive End-to-End documentation for the Navigation System (`WikiCode/SystemeNavigation.md`).
- **Learning**: The Navigation System successfully demonstrates the N-Tier architecture and Bridge pattern, ensuring Core logic (`NavigationService`) generates destinations and calculates costs abstractly. Interaction via Godot Nodes (`NavigationMenu`, `PortalInteraction`) triggers WeakEvents which are intercepted by Godot Singletons (`NavigationManager`) to persist state before deferring transition to `SceneLoadingManager`. This ensures stable decoupling between game state persistence and scene lifecycles.

- **Godot Save Migration & Paths:** Godot's `res://` maps directly to `Src/IslandSurvivor/`. To resolve a path back to the solution root programmatically within Godot, `ProjectSettings.GlobalizePath("res://../../Save/")` successfully breaks out of the Godot project bounds, pointing directly alongside the `.sln`. Using `DirAccess` and `FileAccess` is necessary to gracefully interact with these paths over raw `System.IO` to respect virtual directories (`user://`).

### 2026-04-12 - Inheritance in Godot
- Discovered and addressed that when refactoring existing independent `.tscn` files to utilize inherited scenes, the child `.tscn` needs specific index pathing to safely inject elements (like Map scenes inside the inherited `MapContainer` node) over relying on the standalone architecture.

### 2024-04-16 - Audit Phase 1 - Architectures & Quirks
- **Strict N-Tier Boundaries:** Found purely game-centric entities (`SheepController`, `HealthComponent`) initially residing in the `/Src/Core/` domain. While technically agnostic initially (using `.Numerics`), keeping code specific to visual updates or game loops in the `Core` violates the long-term intent of a clean agnostic layer meant to be shared with ASP.NET APIs. Relocated to `IslandSurvivor/Logic/Entities/` to firmly assert its role as a Godot game component rather than business infrastructure logic.
- **Dependency Nuance in Godot .NET:** Using `System.Numerics.Vector2` in shared libraries is great for purity, but once moved into a Godot Client space (`IslandSurvivor`), relying directly on `Godot.Vector2` proves more cohesive with standard engine workflows and APIs (`Normalized()`, `MoveAndSlide()`, etc).
- **C# Encapsulation vs Atomicity:** A rigid "one class per file" rule must sometimes yield to sensible C# practices. Private nested classes (like `WeakDelegate` inside `WeakEvent`) should not be unnested as it breaks encapsulation and bloats namespaces with components that are functionally irrelevant to the outside system.

### Injection .NET 8 dans Godot 4 (Autoload DI)
- Lors de l'implémentation de `ServiceRegistry` comme conteneur d'injection de dépendances, il est impératif d'utiliser les types primitifs pour les signaux Godot (`[Signal]`). Les événements natifs Godot ne supportent pas les types complexes ou les classes C# pure non-dérivées de `GodotObject`. Pour implémenter le Bridge Pattern, le Bridge (Autoload Godot) doit décomposer les objets Core complexes (ex: `IslandDestination`) en paramètres primitifs (`string`, `int`) lors de la ré-émission des signaux pour que Godot puisse les compiler.

### Audit Phase 3 - Ressources et Signaux (Godot Client)
- **Merge Conflicts in .tres/.tscn :** Des marqueurs de fusion Git (<<<<<<< HEAD) au sein des fichiers texte Godot `.tres` et `.tscn` corrompent le système de parsing interne de Godot. Ils doivent être corrigés au niveau texte via Bash ou l'éditeur car l'UI de Godot ne les surmontera pas.
- **Paramètres p_ :** Une vigilance particulière est requise sur les `delegate` définissant les `[Signal]` en C#. Le parseur de C# vers Godot accepte n'importe quel nom de paramètre, il faut donc s'astreindre soi-même à préfixer par `p_` pour des raisons de conformité.
- **QueueFree() :** Les ressources destructibles (Arbres, Moutons, Minerais) de ce projet appellent bien `QueueFree()` à la fin de leur cycle de vie, garantissant la récupération de la mémoire (Memory Leak Prevention).

### 2025-05-14 - Database Persistence Architecture (N-Tier)
- **Decision**: Implemented a SQL Server schema and EF Core DbContext for meta-progression persistence using strict N-Tier patterns.
- **Architecture**: Domain models (`Player`, `InventoryEntry`, `PlayerStats`, `PlayerConfig`) are defined in `Src/Core/Domain` to ensure the logic layer remains independent of persistence technology.
- **Interfaces**: Introduced `IRepository<T>` in `Src/Core/Interfaces` following the "Interfaces First" principle.
- **Infrastructure**: `AppDbContext` in `Src/Infrastructure` maps Core Domain models using Fluent API. Used GUIDs (`uniqueidentifier`) for primary keys and fixed string ID lengths to match the SQL schema.
- **Documentation**: All persistence documentation is maintained in English (`WikiCode/Persistence_System.md`) to comply with project standards.

### 2025-05-14 - Stats Flexibility via ExtraStats
- **Decision**: Added an `ExtraStats` NVARCHAR(MAX) column to the `Stats` table.
- **Reasoning**: To allow future flexibility for additional stats without requiring database migrations. This column is intended to store JSON data for stats not covered by the main columns (Health, Attack, Speed, Luck).

### 2025-05-14 - Database Naming
- **Decision**: The database for the project is named `DBIslandSurvivor`.
- **Implementation**: Connection strings have been added to the API project's `appsettings.json` and registered in `Program.cs`.

### 2026-04-21 - PlayerConfig Schema Update (Clean-up)
- **Decision**: Removed `Resolution` and `IsFullScreen` fields from `PlayerConfig` domain model, entity model, and database schema.
- **Reasoning**: These fields were deemed unnecessary for the meta-progression persistence via API. Cleaned up the project to maintain only relevant fields and initialized EF Core migrations for the `Infrastructure` project.

### 2026-04-21 - Database Seeding Strategy (US 7.1.2)
- **Decision**: Created a standalone DML script `data.sql` for initial data population.
- **Implementation**: The script seeds `ResourceItems`, `Players`, `Stats`, `PlayerConfig`, and `Inventory`.
- **Constraint**: Base Speed ("vitesse de base") is explicitly set to 1 for all initial players in the `Stats` table as per gameplay requirements.
- **Strategy**: Used T-SQL variables (`@ForgeId`, etc.) to maintain referential integrity across related tables (`Stats`, `Inventory`, `PlayerConfig`) during the seeding process.
### Movement System (US 5.4)
- **Decision:** Maintained movement logic (`MovementController`) inside the `IslandSurvivor` (Godot Client) project rather than moving it to `Core`.
- **Reasoning:** Movement relies heavily on Godot's built-in physics engine and the `MoveAndSlide()` native API to handle collisions and slopes correctly. Implementing a custom 2D physics/collision solver in the C# `Core` would be redundant, error-prone, and suffer a performance hit compared to Godot's optimized C++ implementation.
## Godot Quirks & Physique
- **Collision Layers vs Masks :**
  - **Layer :** Ce que je suis.
  - **Mask :** Ce que je détecte.
- **Configuration IslandSurvivor :** Le Player (Layer 3) ne collisionne pas physiquement avec les objets interactifs (Layer 2), mais son Area2D de détection possède un Mask 2.
- **Signaux Area2D :** Pour émettre body_exited, la propriété monitoring doit être à true.
- **Singletons :** Pour maintenir l'état entre les scènes, InventoryNode utilise l'Autoload Godot combiné à des instances statiques pour son implémentation .NET.

---

## Synchronisation & Meta-Progression (US 8.1)
**Problématique :** Assurer la persistance des données (Stats, Config, Inventaire) de manière sécurisée et optimisée.

**Architecture de Synchronisation :**
- **Sécurité :** Système de "Session Token" (API Key par utilisateur). Le login retourne un token qui doit être inclus dans le corps des requêtes POST ou dans les headers pour les GET.
- **Optimisation (Consolidation) :**
  - `GET /api/player/profile` : Récupère l'intégralité du profil (Joueur, Stats, Inventaire, Config) en un seul appel au lancement.
  - `POST /api/player/sync` : Envoie l'état complet du jeu pour une sauvegarde atomique.
- **Granularité :** Des endpoints individuels (Stats, Inventory) permettent des mises à jour incrémentales durant le gameplay sans surcharger le réseau.
- **Mapping :** Mapping manuel systématique entre les `Entities` (Infrastructure) et les `Domain Models` (Core) pour garantir l'indépendance des couches.

## 2026-04-23: Pub-Sub Bridge Refactor
- Eliminated hybrid `WeakEvent` bridging logic in Core managers in favor of pure `IEvent` payloads published to the `EventBus`.
- `SignalManager` is now exclusively a Godot-side Autoload translator. It listens to Godot Signals and publishes `IEvent`s, and subscribes to `IEvent`s to emit Godot Signals for UI synchronization.
- **Godot Quirk**: Godot signals don't handle C# custom objects well, so complex Core events (`IEvent`) are decomposed into primitive types (int, string) before being emitted as native signals by the `SignalManager`.