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
