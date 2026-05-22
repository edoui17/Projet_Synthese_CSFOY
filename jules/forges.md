[Output truncated for brevity]

  - `GET /api/player/profile` : Récupère l'intégralité du profil (Joueur, Stats, Inventaire, Config) en un seul appel au lancement.
  - `POST /api/player/sync` : Envoie l'état complet du jeu pour une sauvegarde atomique.
- **Granularité :** Des endpoints individuels (Stats, Inventory) permettent des mises à jour incrémentales durant le gameplay sans surcharger le réseau.
- **Mapping :** Mapping manuel systématique entre les `Entities` (Infrastructure) et les `Domain Models` (Core) pour garantir l'indépendance des couches.

### 2026-04-26 - Database Persistence & API Integration
- **Feature**: Implemented API persistence to sync the game state to the remote database using the existing ASP.NET Core API infrastructure.
- **Architecture**: Created `IApiService` and `ApiService` in `Src/Core` to maintain N-Tier strictness. The service uses `HttpClient` to communicate with the `http://localhost:5271` endpoints.
- **Offline Mode**: If the API is unreachable (e.g. `HttpRequestException`), `ApiService` falls back to `ISaveService` (Godot client's local cache via `profile_cache.json`) to persist progression gracefully.
- **Save Event Flow**: Scene transitions via `NavigationManager` now automatically serialize the current `InventoryNode` and `ScoreTracker` state into a `SyncRequest` payload sent to the `/api/player/sync` endpoint, completing the DB roundtrip.

## 2026-04-23: Pub-Sub Bridge Refactor
- Eliminated hybrid `WeakEvent` bridging logic in Core managers in favor of pure `IEvent` payloads published to the `EventBus`.
- `SignalManager` is now exclusively a Godot-side Autoload translator. It listens to Godot Signals and publishes `IEvent`s, and subscribes to `IEvent`s to emit Godot Signals for UI synchronization.
- **Godot Quirk**: Godot signals don't handle C# custom objects well, so complex Core events (`IEvent`) are decomposed into primitive types (int, string) before being emitted as native signals by the `SignalManager`.

### 2026-04-27 - [Architecture - Statistiques Core et Intégration Client]
**Sujet** : Refonte de la classe de statistiques et intégration mathématique dans le client Godot.
**Observation** : L'utilisation d'une classe unique `Stat` pour gérer à la fois les pools (Santé, avec un système Max/Current) et les attributs statiques (Vitesse, Attaque) entraînait une complexité inutile pour ces derniers (qui n'ont pas besoin de limitation ni de régénération). De plus, l'impact de la statistique sur les systèmes du jeu devait être décorrélé de sa valeur absolue en base de données.
**Décision** :
1. **Core N-Tier (Interfaces First)** : Introduction de `IStat`. `Stat` devient `PoolStat` (pour la santé), et ajout de `AttributeStat` (pour les variables statiques). Cette séparation par interface garantit une meilleure évolutivité (ex: on ne pourra pas "soigner" de la vitesse).
2. **Client Godot** : La traduction d'un "point" de statistique en effet réel dans le jeu appartient au client. Le `MovementController` extrait la statistique brute (ex: 1 en Vitesse) et applique la formule mathématique d'impact de gameplay (1 point = +5% de vitesse de base). Cela permet un équilibrage simple côté jeu sans perturber le stockage des valeurs en DB.

### 2026-04-27 - [Architecture - Statistiques Core et Intégration Client - Santé]
**Sujet** : Mise à jour en temps réel de l'UI Godot (Barre de Santé) en réaction à des événements Core via le Bridge Pattern.
**Observation** : L'utilisation de `WeakEvent` pour notifier les composants UI (comme le HUD) depuis le Core violait le modèle d'EventBus établi. L'UI (comme `HealthBarStatic.cs`) contenait en outre de la logique métier (calcul `BaseHealth + Level * HealthPerLevel`) de manière isolée et non synchronisée.
**Décision** :
1. **Core N-Tier** : L'abonnement natif à `OnAnyStatChanged` de `StatTracker` a été remplacé par une émission structurée `m_eventBus.Publish(new StatChangedEvent(...))`.
2. **Client Godot (Bridge)** : Le `SignalManager` (Autoload) s'abonne à `StatChangedEvent` du Core, la décompose en primitives (int, float, float), et émet le `[Signal] StatChanged`.
3. **UI** : L'interface visuelle `HealthBarStatic.cs` obtient ses valeurs d'initialisation via l'injection `ServiceRegistry.Instance.StatTracker`, puis s'abonne uniquement au `SignalManager`. Cette approche permet à l'UI de rester "stupide" et de se contenter d'afficher les valeurs réelles calculées par la couche métier.

### 2026-04-28 - Intégration des Statistiques : AttributeStat vs PoolStat
- **Découverte/Observation :** L'architecture du `StatTracker` sépare explicitement les types de statistiques en deux implémentations : `PoolStat` (ex: Santé) et `AttributeStat` (ex: Vitesse, Attaque, Chance).
- **Détails Techniques :**
  - `PoolStat` possède une notion de valeur courante et valeur maximale effective, idéale pour les jauges. L'ajout d'un bonus augmente la limite maximale et restaure proportionnellement la valeur courante.
  - `AttributeStat` s'incrémente linéairement. Elle n'impose pas de "plafond", l'ajout d'un bonus incrémente la stat actuelle sans se soucier du calcul des pourcentages par rapport à un maximum.
  - La logique s'intègre parfaitement aux tests xUnit (`AddPermanentBonus_UpdatesMaxAndCurrentSimultaneously` vs `AttributeStat_IncrementsCorrectly_WithoutMaxLogic`), où `StatType.Luck` suit exactement le comportement d'`AttributeStat`.
  - Lors de l'influence de la "Chance" (Luck) sur le butin dans Godot, les valeurs de statistiques sont lues depuis la logique `Core` (`ServiceRegistry.Instance.StatTracker.GetCurrentValue(StatType.Luck)`) afin de préserver l'architecture propre, plutôt que de dépendre de Godot.

### 2026-04-28 - Decentralized Stat Tracking and Godot Signals
- **Discovery**: Relying on a global `ServiceRegistry.Instance.StatTracker` caused all entities to share exactly the same health, making independent combat interactions impossible.
- **Technical Detail**: The solution leverages pure C# composition combined with Godot Signals. The Godot `StatManager` node was refactored to spawn its own *local* `EventBus` and `StatTracker` upon `_Ready()`, creating true instances per entity. To communicate updates up to Godot components (like floating HP bars) without polluting the global `SignalManager`, `StatManager` listens to the C# `StatChangedEvent` on its isolated bus and re-emits a `[Signal] LocalStatChanged`. This keeps Godot UI components completely agnostic of Core interfaces while preserving N-Tier boundaries per entity.
### 2026-04-30 - Decentralized Stat Tracking and Godot Signals
- **Discovery**: Relying on a global `ServiceRegistry.Instance.StatTracker` caused all entities to share exactly the same health, making independent combat interactions impossible.
- **Technical Detail**: The solution leverages pure C# composition combined with Godot Signals. The Godot `StatManager` node was refactored to spawn its own *local* `EventBus` and `StatTracker` upon `_Ready()`, creating true instances per entity. To communicate updates up to Godot components (like floating HP bars) without polluting the global `SignalManager`, `StatManager` listens to the C# `StatChangedEvent` on its isolated bus and re-emits a `[Signal] LocalStatChanged`. This keeps Godot UI components completely agnostic of Core interfaces while preserving N-Tier boundaries per entity.

# Forge Technical Log

## 2026-04-24 - Line of Sight Implementation
- **Quirk/Discovery:** When implementing `RayCast2D` checks in the `_PhysicsProcess`, it is important to call `ForceRaycastUpdate()` after modifying `TargetPosition` to ensure the collision check is accurate for the current frame before evaluating `.IsColliding()`. This prevents off-by-one frame lag in detection.
- **Quirk/Discovery:** Godot will throw `can_instantiate: Cannot instantiate C# script because the associated class could not be found` if a pure C# class (like `AgressorController` that does not inherit from `Node`) is attached directly to a node in the `.tscn` file. Pure logic scripts must be instantiated manually in the C# script of the node they belong to (e.g., `_logic = new AgressorController()`).
- **Quirk/Discovery:** When using `RayCast2D` for obstacle detection, if `IsColliding()` is checked, it will hit *anything* on its Collision Mask. Therefore, if the RayCast is meant to detect walls *between* the enemy and the player, it needs to explicitly check if the hit `GodotObject` is the player. If it hits something else, it's an obstacle. If the `TargetPosition` is set to the player's position, and the ray hits *nothing*, it could mean the player is out of range, or the ray doesn't interact with the player's layer but reached the target without hitting a wall.

### RayCast2D TargetPosition Quirk
When adjusting a `RayCast2D`'s `TargetPosition` via code attached to a parent node to point toward a global target (like the Player), you must convert the target's global position into local coordinates. `TargetPosition` uses the local coordinate space of the RayCast itself.
**Incorrect:** `Vector2 targetDirection = target.GlobalPosition - GlobalPosition;` (This breaks when parent nodes rotate or move).
**Correct:** `Vector2 targetLocalPosition = ToLocal(target.GlobalPosition);` (Assuming the RayCast2D is at 0,0 relative to the script's parent).

## 2026-05-07 - Signal-Based Attack Logic vs Area Polling
- **Quirk/Discovery:** In Godot, when activating a `CollisionShape2D` hitbox mid-animation via `AnimationPlayer` (e.g., turning `disabled` off at 0.2s), polling for overlapping areas manually in the same C# function call using `GetOverlappingAreas()` will fail if called instantly.
  - Using `await ToSignal(GetTree().CreateTimer(0.25f), SceneTreeTimer.SignalName.Timeout)` and then `GetOverlappingAreas()` works but can feel brittle.
  - The more idiomatic Godot solution is relying on the signals `AreaEntered` and `BodyEntered` emitted natively by the `Area2D` when the `disabled` flag flips to `false` during the animation frame.
- **Architectural Shift:** Moving from a procedural execution list to an event-driven `HashSet<IDamageable>` tracking mechanism ensures single-hits per target per attack frame while leveraging Godot's built-in physics event queue.

## 2026-05-08 - API & DB Audit
- **Security Discovery:** Plain text password storage is temporarily accepted for development validation, but the architecture is ready for BCrypt integration via `IAuthRepository`.
- **Architectural Shift:** Introduced `AuthResponse` and `ProfileResponse` DTOs in the Core layer. This ensures that Database Entities (Infrastructure) never leak into the API responses, maintaining a strict N-Tier separation and preventing accidental exposure of sensitive fields like `PasswordHash`.
- **Database Quirk:** EF Core `HasIndex(e => e.Username).IsUnique()` is essential even if the database has a `UNIQUE` constraint, as it allows EF to optimize queries and handle validation at the tracking level.

## 2026-05-09 - Infrastructure & Mapping Update
- **SQL Server Instance**: Migrated from LocalDB to SQL Server Developer (MSI). Connection strings are updated to target `Server=.` with `TrustServerCertificate=True`.
- **Database Reset Procedure**: Modified `schema.sql` to include a database recreation header (USE master -> DROP -> CREATE) to ensure a clean slate for every deployment.
- **Type Mapping Fix**: All floating-point columns (Health, Attack, Speed, Luck, etc.) are converted from `FLOAT` to `REAL` in the database schema. This prevents `InvalidCastException` when mapping 64-bit SQL floats to 32-bit C# floats.
- **Environment**: Formalized Visual Studio (Full) as the primary development IDE.

## 2026-05-09 - Deployment & Schema Lifecycle Standard
- **Deployment Reliability**: To ensure "zero friction" deployment, each developer is instructed to customize the `Server=` parameter in their local `appsettings.json` to match their SSMS instance (e.g., `Server=MSI`).
- **Schema Robustness**: `schema.sql` now includes `IF OBJECT_ID(...) DROP TABLE ...` clauses for all project tables. This prevents re-initialization failures due to existing foreign key constraints or lingering metadata.
- **SQL Server Instance**: Re-confirmed SQL Server Developer Edition (MSI) as the project's baseline standard.
### 2026-05-08 - Dynamic Property Hiding in Godot C#

To dynamically hide exported properties in the Godot Inspector using C#, the Node must be marked with the `[Tool]` attribute, and it must override the `_ValidateProperty(Godot.Collections.Dictionary property)` method. Inside `_ValidateProperty`, clear the `PropertyUsageFlags.Editor` flag on the property `usage` when conditions are met.

```csharp
[Tool]
public partial class MyNode : Node
{
    private int m_type;
    [Export]
    public int Type
    {
        get => m_type;
        set
        {
            m_type = value;
            NotifyPropertyListChanged();
        }
    }

    [Export] public int HiddenProperty { get; set; }

    public override void _ValidateProperty(Godot.Collections.Dictionary property)
    {
        if (!Engine.IsEditorHint()) return;

        string name = property["name"].AsString();
        if (name == "HiddenProperty" && m_type == 0)
        {
            var usage = property["usage"].As<PropertyUsageFlags>();
            property["usage"] = (int)(usage & ~PropertyUsageFlags.Editor);
        }
    }

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) return;
        // Game logic
    }
}
```

*Note:* Wrapping the conditional properties triggering a hide/show check within an explicit property allows calling `NotifyPropertyListChanged()` upon modification, instantaneously updating the Inspector. Ensure all runtime logic within `_Ready`, `_Process`, etc., starts with `if (Engine.IsEditorHint()) return;` to prevent execution in the editor.

### 2026-05-09 - Testing EventBus Event Side-Effects
When triggering updates (e.g., UI upgrades emitting events to a decoupled component via `EventBus`), reading back the updated values immediately within the same method frame might fail. The global `EventBus` processes its subscription queue inside its `_Process` loop, meaning any data change side-effects will be deferred. To accurately log or verify the "after" state in Godot C# test scenes, execution must be yielded by `await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);` to allow the EventBus to iterate and subscribers to update their state.

## 2026-05-09 - Configuration Consolidation & API Security
- **Discovery**: Maintaining hardcoded API keys in the source code (`ApiService.cs`) creates security risks and deployment friction.
- **Refactoring**:
  - **Core**: `ApiService` constructor was updated to receive the API Key as a dependency, decoupling it from a hardcoded constant.
  - **Godot (IslandSurvivor)**: The API Key is now stored in `project.godot` under `network/api/api_key` and retrieved via `ProjectSettings`.
  - **Web**: The API Key is stored in `appsettings.json` and injected into the `HttpClient` instance at registration time in `Program.cs`.
  - **Cleanup**: Redundant/commented-out code in `PlayerController.cs` was removed to maintain API cleanliness.
- **Technical Detail**: In Godot C#, using `ProjectSettings.GetSetting("path").AsString()` is the standard way to access custom configuration defined in the `project.godot` file, allowing for environment-specific overrides during export.

## 2024-05-24 - Architecture Changes
- **Spawning Logic:** Clarified that the procedural spawning algorithms (e.g., `ResourceZone`) must reside within the Godot client (`IslandSurvivor`) as they are tightly coupled to the engine's 2D math (`Vector2`, `Geometry2D`, `TileMapLayer`) and do not impact Core backends. Refactored `ResourceZone` to adhere to DRY principles by extracting validation logic into discrete methods.
- **Dead Code Elimination:** Removed all orphaned procedural generation files (`MapManager`, `GodotIslandGenerator`, `SpawnLocator`, `MapRenderer`, etc.) as the project transitioned entirely to hand-crafted maps.
- **Event Architecture Enforcement:** Completely removed `WeakEvent` implementations from the Core layer. Classes like `AttributeStat` and `PoolStat` now strictly communicate via the global `IEventBus` using POCO `IEvent`s (`StatChangedEvent`).

### Architect Log - Implementing Enemy Hitboxes and Delays
- **Hitbox Implementation**: In `EnemyBase.cs`, replacing a single static `HitboxArea` with `HitboxAreaRight` and `HitboxAreaLeft` allows directional attacking based on the enemy sprite's `FlipH` property.
- **Asynchronous Attacks**: Introduced an asynchronous `HandleAttackState()` using `await ToSignal(GetTree().CreateTimer(0.4f), SceneTreeTimer.SignalName.Timeout)` to simulate attack wind-up. Caution: Always check if the entity died (`CurrentState == NpcStates.DEAD`) during the await period before applying damage to prevent null reference exceptions or ghost attacks.

### Architect Log - N-Tier Inheritance Refactoring
- **EnemyBase Refactoring**: To follow best architecture practices, `EnemyBase` was abstracted. It now only contains common entity behavior (movement, scaling, stats, line of sight).
- **Subclasses (`MeleeEnemyBase` & `RangedEnemyBase`)**:
  - Melee behaviors (like multiple `HitboxArea` monitoring and `m_playersInHitbox` tracking) are now exclusively inside `MeleeEnemyBase` (inherited by `Soldier`).
  - Ranged behaviors (projectile instantiating, distance checking) are exclusively inside `RangedEnemyBase` (inherited by `Archer`).
## Technical Log - Enemy State Machine & Movement Quirks

- **Catch-22 in State Evaluation**: When using custom logic controllers (like `IAgressorController`), avoid wrapping state transition triggers (like checking if the player is in the hitbox to start an attack) inside an `if` statement that checks if the enemy is *already* in the target state. In `EnemyBase.cs`, `HandleAttackState()` was locked behind `if (CurrentState == ATTACK)`, making it impossible to enter the attack state from the CHASE state.

- **Physics Frame Continuity**: When interrupting movement to perform an action (like attacking), it's generally better to set the target speed and direction vector to zero and let the script flow down to `MoveAndSlide()` rather than using an early `return;`. This ensures that Godot's physics engine still processes the frame and resolves external collisions (e.g., being pushed by another entity) even while the character is seemingly standing still.

## Audio and Visual Feedback Architecture
- **Audio Global/Spatial Handling**: Created `AudioManager` singleton attached to the root, pooling `AudioStreamPlayer` (Global/UI) and `AudioStreamPlayer2D` (Spatial) to decouple sounds from node lifetimes. Prevents sounds cutting off prematurely when entities (like resources or enemies) queue free upon death.
- **Node Tweening Extensions**: Added `PlayShake` extending `Node2D` using Godot's `Tween` API to systematically implement camera shakes and entity impact hits without polluting entity logic.
## 2026-05-14 - Meta-Progression & HighScore Refactoring (US 18.0)
- **Database Evolution**: Migrated the `Stats` table from a 1-to-1 relationship with `Players` to a 1-to-many relationship under the new name `GameStats`.
- **Relationship Quirk**: Moving from 1-to-1 to 1-to-many required updating the `PlayerEntity` navigation property to `ICollection<GameStatsEntity>`. This allows tracking full session history.
- **SQL Trigger Logic**: Implemented `TR_GameStats_AfterInsert` directly in the EF Core migration. The trigger automatically updates the `HighScore` in the `Players` table and prunes the `GameStats` history to keep only the top 10 sessions per player.
- **Mapping Duration**: Mapped the SQL `TIME` type to C# `TimeSpan`. Note: EF Core handles this natively, but Ensure the column is defined as `TimeSpan` in the Entity for proper mapping to the `TIME` SQL type.
- **Data Strategy**: Opted for a 'Clean Slate' approach for the database deployment. The migration was simplified to directly create the new structure, and SQL scripts (`schema.sql`, `data.sql`) were updated accordingly.
- **API Strategy**: Updated `Sync` endpoint to append new sessions to `GameStats` rather than overwriting a single record. The `Profile` endpoint now returns the `HighScore` and the list of best sessions.

## 2026-05-14 - Progression Intelligence & API Refactoring (Task 18.0.2)
- **Architectural Shift**: Introduced `IProgressionService` in the `Core` layer to centralize game logic (level calculation, high score validation). This removes business logic from Controllers, adhering to strict N-Tier principles.
- **Level Progression**: Implemented a threshold-based level system (Level 1: 0-999, Level 2: 1000-2499, Level 3: 2500-4999, Level 4+: +5000 per level). Centralizing this in a service allows for easy future balancing.
- **Consistency Strategy**: While a SQL trigger handles DB-level HighScore integrity, the `ProgressionService` updates the `Player` object in memory during the request. This ensures the API response (e.g., during `Sync`) contains the most up-to-date HighScore immediately.
- **Leaderboard Performance**: Refactored `GetLeaderboard` to sort by the `HighScore` column in the `Players` table. This is O(1) or O(log N) with indexes, compared to the previous O(N*M) approach of scanning all session history.

## 2026-05-15 - Leaderboard Filtering & Sorting (US 10.1.5)
- **POCO Enrichment**: Updated `PlayerLeaderboardEntry` to include `Duration`, `BonusHealth`, `BonusAttack`, `BonusSpeed`, and `BonusLuck`. This allows the frontend to display detailed performance metrics regardless of the active filter.
- **Dynamic Sorting Logic**: Implemented a `switch` based sorting mechanism in `PlayerController.GetLeaderboard`.
  - Sorting criteria include: `score`, `duration`, `level`, and bonus stats (`health`, `attack`, `speed`, `luck`).
  - For session-based metrics (duration, level, bonuses), the API calculates the `Max()` value across the player's `GameStats` collection to determine their ranking.
- **Search Functionality**: Added a `p_search` parameter for "Contains" (case-insensitive) filtering on `Username`, anticipating Scénario 3.
- **Data Integrity**: Maintained the "Best Session" projection. Even when sorting by a specific stat (e.g., Speed), the returned entry displays the full stats from the player's highest-scoring session to ensure a coherent "Master Profile" view.
## 2026-05-16 - Attack Cooldowns and Action Mechanics (US Gameplay Loop)
- **Continuous Actions**: Godot's `_Input(InputEvent)` is strictly event-driven (e.g. key pressed/released). For continuous actions (like holding down the mouse button to attack), logic must evaluate `Input.IsActionPressed()` every frame inside `_PhysicsProcess` or `_Process`.
- **Stat-Bound Cooldown Strategy**: Action cooldowns (e.g., Attack cooldowns) should scale down based on progression stats (like `Speed`) using the asymptotic formula: `Cooldown = BaseCooldown / (1 + (Speed * 0.05))`. This ensures the cooldown never mathematically hits 0, preventing infinite DPS loops regardless of the stat ceiling.

## 2026-05-17 - Architecture N-Tiers, Règles Globales & CI/CD Unifié (Audit)
- **Frontières N-Tiers Strictes** :
  - **Core** : Couche "Comptable". Doit être 100% agnostique. Interdiction stricte de référencer Godot (`Godot.*`) ou Entity Framework. C'est ici que résident l'isolation mathématique (`CombatMath`, formules XP) et les abstractions (`IEventBus`, interfaces de services).
  - **IslandSurvivor (Godot)** : Couche "Orchestrateur". Interdiction d'accès direct à l'infrastructure (DB). Elle gère le moteur, les noeuds (`Nodes`), la boucle de jeu locale, le visuel et notifie le Core via des événements.
  - **API** : Couche d'Exposition. N'altère aucune logique métier. Sert de pont sécurisé entre le Client et la Base de données.
  - **Infrastructure** : Couche Persistance. Seule autorisée à manipuler Entity Framework et le SQL. Dépend du Core pour les définitions métiers.
  - **Web** : Couche Dashboard. Dépend du Core. Ne communique jamais avec la base de données directement ; consomme l'API.
- **Règles Globales de Gameplay** :
  - **Combat & Factions** : La logique temporelle (cooldowns), les hitboxes et le ciblage par `EntityFaction` (Joueur vs Ennemi) sont strictement isolés dans le Godot client (`AttackController`). L'animation (`AnimatedSprite2D`) pilote l'état visuel de l'attaque.
  - **Projectiles** : Entrent en collision physique avec le décor, mais n'appliquent des dégâts logiques qu'aux cibles correspondant à leur `EntityFaction`.
  - **Dash & Interruptions** : Recevoir des dégâts d'un projectile pendant un Dash l'interrompt immédiatement et applique un statut *Stun* via le `MovementController`.
  - **Cooldowns** : Le scaling se fait via une formule asymptotique (ex: `Cooldown = BaseCooldown / (1 + (Speed * 0.05))`) centralisée en C# pour éviter les boucles infinies.
- **CI/CD Unifié (Pipeline YAML)** :
  - Le pipeline doit obligatoirement inclure un Stage de **Validation** (restauration, compilation globale `ProjetJeu.sln`, et `dotnet test` sur `Tests/`) avant de permettre le Stage de **Build & Export** Godot.
  - Une tolérance d'erreur est requise sur l'étape de pré-importation headless de Godot (`exit 0` forcé) pour permettre la génération du `.godot/` sans échouer prématurément sur les erreurs C# initiales.

## 2026-05-18 - Inspection Symétrique & Extraction du Couplage Gameplay
- **Constat Critique :** L'inspection du `Core` a révélé que des interfaces actives de gameplay (`IAgressorController`, `IRangedController`, `IProjectile`), utilisant `System.Numerics.Vector2` pour contourner la restriction `Godot.*`, s'étaient infiltrées dans le projet. Ces interfaces encapsulaient de la logique spatio-temporelle (direction, poursuite, calcul de trajectoire).
- **Correction Architecturale :** Le `Core` étant un "noyau mathématique pure", toute interface ou classe pilotant activement le temps (`_Process`), l'espace (`Vector2`) ou les cycles de vie des entités moteurs a été expulsée. Elles résident désormais légitimement dans `IslandSurvivor/Logic/Entities/` et consomment le vrai `Godot.Vector2`.
- **Règle d'or :** Si un algorithme a besoin de connaître un vecteur (`X, Y`) pour interpoler un mouvement en temps réel, ou s'il dépend du `DeltaTime`, il **n'a pas sa place** dans le projet `Core`.

## 2026-05-18 - Stratégie d'Optimisation des Collections (C# vs Godot)
- **Constat Technique :** Le marshalling des données entre le domaine managé de C# (.NET) et le cœur natif en C++ de Godot a un coût de performance non négligeable.
- **Règle d'Architecture :**
  - **`System.Collections.Generic` (List, Dictionary, etc.) :** Doit être le standard absolu pour 100% de la logique interne, des calculs du `Core`, et de la gestion de l'état (inventaires, statistiques). Cela permet de conserver des performances natives C# et un accès complet à LINQ.
  - **`Godot.Collections.Array<T>` / `Dictionary` :** Strictement réservés à la couche d'orchestration (`IslandSurvivor`) et uniquement dans deux scénarios précis :
    1. Pour exposer des tableaux dans l'Inspecteur Godot via l'attribut `[Export]`.
    2. Pour appeler des méthodes de l'API Godot qui requièrent explicitement ces types de retour.
- Cette ségrégation garantit que le projet `Core` reste totalement agnostique et hautement performant.

## 2026-05-18 - Blazor Authentication & Navigation Guards
 - **Quirk/Discovery:** In a Blazor Web setup using `AddAuthorizationCore()` without full ASP.NET Identity, the `AuthorizeRouteView` component enables the use of `[Authorize]` attributes and `<AuthorizeView>` tags, but it does not automatically perform redirects for unauthorized access.
 - **Solution:** Manual navigation guards within the `OnInitializedAsync` method (by checking `GetAuthenticationStateAsync`) are necessary to enforce redirects to `/login`.
 - **API Interception:** Implementing a `DelegatingHandler` for the `HttpClient` is the most efficient way to centralize the injection of the `x-Session-Token` header and to handle global `401 Unauthorized` responses (e.g., by clearing LocalStorage and redirecting the user).

## 2026-05-20 - API Resilience and Audit Synchronization (US 17.0.2)
- **Standardized Error Handling**: Enhanced `ExceptionHandlingMiddleware` to intercept infrastructure failures (e.g., `SqlException`). It now returns a structured JSON response: `{ "error": "...", "message": "...", "timestamp": "..." }` with a `503 Service Unavailable` status. This prevents the Blazor client from receiving HTML error pages and allows for clean UI alerts.
- **Audit Traceability**: Added the `UpdatedAt` timestamp to the `ProfileResponse` POCO. This value is mapped from the `Player` entity (updated via SQL trigger) to allow the dashboard to display the "Last Synchronization" time.
- **Eager Loading Optimization**: Removed `.Include(p => p.GameStats)` from generic repository methods in `PlayerRepository` and `AuthRepository`. Since session history can grow indefinitely, loading the entire collection during every profile fetch or authentication check is inefficient. The API now relies on `StatsRepository.GetTopStatsByPlayerIdAsync(p_count: 10)` for targeted loading, maintaining performance without sacrificing data availability.

## 2026-05-21 - Blazor Lifecycle and API Integration (US 17.0.3)
- **Lifecycle Management**: Integrated `OnInitializedAsync` in `Dashboard.razor` to handle data fetching during the Blazor component's initialization.
- **Visual State Management**: Implemented a tri-state UI (Loading, Error, Success) using boolean flags (`m_isLoading`) and error message strings. This ensures Scénarios 2 and 5 are handled gracefully.
- **Data Binding & Null Safety**: Used null-conditional operators (`?.`) and fallback values (e.g., `?? "0.0"`) when binding `ProfileResponse` to the UI. This prevents runtime exceptions if the player has no session history.
- **UI Architecture**: Leveraged Bootstrap 5 for a responsive dashboard, including a fixed-top style header and a scrollable session history table.

## 2026-05-21 - API Error Standardization & UTC Synchronization (US 20.0.2)
- **Centralized Error Handling**: Created `ErrorResponseHelper` in `Src/API/Utils` to unify the `{ error, message, timestamp }` JSON format. This reduces duplication across `ApiKeyMiddleware`, `SessionAuthMiddleware`, and `ExceptionHandlingMiddleware`.
- **Infrastructure Mapping (503)**: Refined `ExceptionHandlingMiddleware` to catch `SqlException` and `DbUpdateException`, returning a `503 Service Unavailable` status. This informs the Godot client that the failure is at the persistence layer rather than a logic error.
- **ISO 8601 / UTC Compliance**: In `PlayerController.GetProfile`, used `DateTime.SpecifyKind(player.UpdatedAt, DateTimeKind.Utc)` before assignment. This ensures the .NET JSON serializer appends the `Z` suffix, which is critical for Godot's `Time.get_datetime_dict_from_datetime_string()` parser.
- **Security & Whitelisting**: Tightened `SessionAuthMiddleware` by switching from `Contains` to `StartsWith` for path whitelisting (e.g., `/api/auth/login`, `/swagger`) to prevent bypasses via crafted query parameters.

## 2026-05-24 - Game Session Management & Persistence (US 20.0.1)
- **State Machine**: Implemented a global `GameManager` (Autoload) using the `AppStatus` enum (Loading, Ready, Error). This decouples application lifecycle from service initialization (`ServiceRegistry`).
- **Persistence (user://)**: Introduced `SessionProvider` using Godot's `ConfigFile` specifically for `user://session.cfg`. This ensures the session token remains persistent and OS-compliant in exported builds, unlike project-root relative paths.
- **Access Control**: The `GameManager` validates the stored token at launch. If invalid or missing, it forces redirection to `Login.tscn`.
- **Offline Fallback**: The `ErrorPopup` handles API unreachable states by offering a "Play Offline" mode, which sets the `GameManager` to `Ready` state with a "Guest" flag, bypassing mandatory authentication for local play.

## 2026-05-24 - API Data Mapping and Injection (US 20.0.3)
- **Profile Synchronization Architecture**: Implemented 'ProfileResponse' as the network source of truth, refactoring 'IApiService.GetProfileAsync' to use it.
- **Mapping & Domain Integrity**: Introduced 'ProfileMapper' (Core.Utils) to convert 'ProfileResponse' DTOs into the 'PlayerProfile' aggregate. This maintains a strict N-Tier separation while allowing the Godot engine to remain agnostic of API DTO structures.
- **Null-Safe Deserialization**: Ensured that 'ProfileMapper' and 'ApiService' provide safe fallback collections ('new List<T>()') if JSON fields are missing or null, preventing 'ArgumentNullException' during initialization.
- **Event-Driven Initialization**: Hooked 'GameManager' into the 'ProfileLoadedEvent'. Upon successful profile fetch (from network or cache), the 'GameManager' publishes this event to the 'EventBus'.
- **Inventory & Stat Sync**: 'InventoryManager' and 'StatManager' were updated to subscribe to 'ProfileLoadedEvent'. This triggers an idempotent 'InitializeInventory' call and a full stat override respectively, ensuring the player character reflects their remote progression immediately upon loading.
- **Service Resilience**: Updated 'IApiService' to expose 'GetCachedProfile()', allowing the 'GameManager' to retrieve last-known data without violating N-Tier constraints via implementation casting.
- **Namespace Management**: A naming conflict exists between 'Core.Domain.Player' (Domain model) and 'IslandSurvivor.Scenes.Player.Player' (Godot CharacterBody2D). Code in the Godot project must use fully qualified names (e.g., 'Core.Domain.Player') to avoid build errors (CS0117).

## 2026-05-25 - US 20.0.4 : Architecture de l'Écran de Chargement et UX de Synchronisation

### Découvertes Architecturales
- **Gestion du ProcessMode au Démarrage** : Pour bloquer efficacement les entrées avant le chargement de la première scène de gameplay, le `GameManager` (Autoload) doit appliquer `ProcessModeEnum.Disabled` sur la `CurrentScene` du `SceneTree`.
- **Découplage UI/Logique via Signaux** : L'utilisation de signaux personnalisés (`RetryRequested`, `OfflineModeRequested`) dans `LoadingScreen.cs` permet d'éviter un couplage fort avec le `GameManager`, facilitant la maintenance et les tests.
- **Blocage des Inputs via CanvasLayer** : Un `CanvasLayer` avec une couche élevée (e.g., 128) et un `ColorRect` ayant `MouseFilter = Stop` est la méthode la plus robuste pour intercepter tous les événements d'entrée dans Godot 4.

### Quirks Godot/C#
- **Rotation Procédurale** : La rotation du spinner dans `_Process` doit utiliser `delta` pour assurer une fluidité constante indépendamment du framerate.
- **Transition de Scène et ProcessMode** : Lors de l'appel à `ChangeSceneToFile`, la nouvelle scène est chargée avec son propre `ProcessMode` (généralement `Inherit`), ce qui réactive implicitement le traitement du jeu après la disparition de l'écran de chargement.

## 2026-05-25 - US 20.0.5 : Sécurité, Résilience et Validation Anti-Cheat

### Architecture de Validation et Anti-Cheat
- **Validation Défensive** : Introduction de `ProfileValidator` (Core.Utils) pour intercepter les données de profil aberrantes avant leur injection dans le moteur.
- **Seuils de Sécurité** :
  - Santé (Health) : doit être > 0 et <= 100.
  - Attaque, Vitesse, Chance (Attack, Speed, Luck) : doivent être >= 0 et < 999.
- **Comportement sur Échec** : En cas de détection de valeurs invalides, le client journalise une alerte d'intégrité et bascule automatiquement sur le profil "Guest" par défaut pour protéger l'expérience de jeu.

### Résilience et Protection contre le Spam (Backoff)
- **Retry Backoff** : Implémentation d'un mécanisme de temporisation linéaire sur le bouton de tentative de reconnexion (`RetryRequested`).
- **Formule de Délai** : `Délai = min(3 * m_retryAttempt, 15)`. Le compteur est réinitialisé lors d'une synchronisation réussie.
- **Feedback UX** : Le bouton Retry est désactivé durant le cooldown et affiche un compte à rebours dynamique : "Réessayer ({0}s)".

### Transition Hors Ligne Transparente
- **Feedback de Chargement** : L'utilisation de "Continuer Hors Ligne" déclenche un message d'information temporaire ("Connexion impossible. Lancement en mode hors ligne avec les données locales...") pendant 1.5 seconde via `ToSignal(GetTree().CreateTimer(1.5f), "timeout")`, assurant une UX fluide et transparente.

### Standardisation de l'Authentification et Infrastructure
- **Validation des Credentials** : Le flux de Login du jeu valide systématiquement les identifiants (Username/Password) auprès de l'endpoint d'authentification de l'API (`/api/auth/login`) avant d'autoriser l'accès aux fonctionnalités en ligne.
- **Dépendance SQL Server** : Le serveur de base de données (SQL Server) doit être actif et accessible par l'API pour permettre l'authentification initiale et l'obtention du `SessionToken`.
- **Gestion de l'Indisponibilité (503)** : Si la base de données est arrêtée ou inaccessible, l'API renvoie une erreur 503 (via `ExceptionHandlingMiddleware`). Le client intercepte cette erreur, lève une alerte visuelle et propose le basculement vers le mode hors ligne basé sur le cache local (`user://session.cfg` et `profile_cache.json`).
