# Forge Architectural Log

## 2026-03-22 - Player Interaction System
- **Closest Object Prioritization**: Implemented in `InteractionService` (Core). Uses a simple distance calculation between the player's global position and interactable candidates.
- **IInteractable Interface**: Defined in Core to allow any object (NPC, Item, Portal) to be interactable without engine dependencies.
- **InteractableNode**: A Godot `Area2D` bridge that registers itself with the player's detection system and delegates the `Interact()` call to its logic.

## 2026-03-22 - Combat System (Attack)
- **IAttackable Interface**: Added to allow nodes to receive damage.
- **Attack Controller**: Manages the weapon's hitbox lifecycle synchronized with the `AnimationPlayer`.
- **State Blocking**: Movement is programmatically blocked when `PlayerState == Attacking`.

## 2026-04-19 - Persistence System Architecture (N-Tier)
- **Model Sharing**: Domain models moved to `Src/Core/Domain` to be shared by all layers.
- **Infrastructure Layer**: Exclusively handles EF Core and SQL Server. Repositories map internal entities to domain models.
- **Inventory Junction**: Implemented a junction table for Inventory to link Players and ResourceItems.

## 2026-04-20 - Meta-Progression & SQL REAL
- **Stat Persistence**: Meta-progression (Bonus Attack, Speed, etc.) is stored in the `Stats` table.
- **SQL REAL Type**: All floating-point numbers in SQL are now `REAL` (32-bit) to ensure perfect mapping with C# `float` and avoid `InvalidCastException`.
- **ExtraStats Column**: Added an `NVARCHAR(MAX)` column to `Stats` for future JSON-based extensions without schema changes.

## 2026-04-21 - PlayerConfig Consolidation
- **Scope**: Removed `Resolution` and `IsFullScreen` from persistence as they are client-specific and not part of meta-progression.
- **EF Core Migrations**: Initialized the migration system to manage schema changes safely.

## 2026-04-22 - Session-Based Authentication & Profile Sync
- **Security**: Implemented `PasswordHash` (BCrypt style) and `SessionToken` (GUID) in the `Players` table.
- **Consolidated Sync**: Created a single `ProfileResponse` DTO to fetch all meta-progression (Stats, Inventory, Config) in one network round-trip.
- **API Performance**: Transitioned from granular individual GETs to a consolidated profile endpoint to optimize game startup time.

## 2026-05-08 - API Security and Data Integrity
- **Unique Constraint**: Added a unique constraint to `Username` in SQL and EF Core.
- **N-Tier POCOs**: Standardized on pure POCOs for all API requests and responses, ensuring no internal entities are exposed.
- **Leaderboard Logic**: Fixed the leaderboard to correctly project the best session for each player.

## 2026-05-09 - Resilience and Infrastructure Security
- **DB Resilience**: Configured `EnableRetryOnFailure` in EF Core.
- **API Key**: Implemented `ApiKeyMiddleware` (`X-API-KEY`) for an initial layer of security.
- **Fallback Strategy**: Documented that the Godot client must use its local cache if the API returns a 503 error.

## 2026-05-09 - Environment and Type Safety
- **SQL Standard**: Confirmed `REAL` as the project standard for all floating-point columns.
- **Reset Script**: The `schema.sql` now includes a full database drop/create sequence for clean developer environments.
- **Connection Strings**: Standardized on `TrustServerCertificate=True` for local SQL Developer instances.

## 2026-05-14 - Session History & Automatic HighScore
- **Schema Evolution**: `Stats` table renamed to `GameStats` with 1-N relationship to support session history.
- **Database Trigger**: Implemented `TR_GameStats_AfterInsert` to automatically update `Player.HighScore` and prune history to the top 10 sessions.
- **Time Representation**: Standardized on SQL `TIME` and C# `TimeSpan` for game duration tracking.

## 2026-05-14 - Progression Service & API Calculations
- **Service Layer**: Introduced `ProgressionService` (Core) to centralize leveling logic and high score validation.
- **Stat Consistency**: The API now updates the `Player` profile in memory before responding, ensuring the client receives the most up-to-date high score immediately.
- **Leaderboard Performance**: Optimized queries to use the indexed `HighScore` column in the `Players` table.

## 2026-05-15 - Dynamic Leaderboard Filtering
- **Query Projection**: The leaderboard now projects the player's absolute best session (Master Session) even when filtering by other metrics like Health or Luck.
- **Case-Insensitive Search**: Implemented case-insensitive username filtering in SQL for the leaderboard UI.
- **POCO Enrichment**: Added bonus stats and duration to `PlayerLeaderboardEntry` to support rich dashboard visuals.

## 2026-05-18 - Blazor Authentication & Token Persistence
- **Native Security**: Used `AuthorizeRouteView` and `[Authorize]` for UI protection.
- **LocalStorage**: Implemented token persistence via `IJSRuntime` to avoid third-party library dependencies.
- **Interception**: Created `SessionTokenHandler` (DelegatingHandler) to automatically inject headers and handle `401 Unauthorized` globally.

## 2026-05-21 - Dashboard UX & Error Resilience
- **Tri-State UI**: Implemented Loading, Error (503), and Success states in `Dashboard.razor`.
- **Sync Audit**: Displayed `UpdatedAt` with UTC compliance to allow players to verify their last synchronization.
- **History Projection**: Optimized the dashboard to project the "Best Session" from the session history for the main stats summary.

## 2026-05-24 - Godot Session Management & GameManager State
- **Config persistence**: Session tokens are stored in `user://session.cfg` to ensure persistence across exports.
- **AppStatus State Machine**: GameManager uses Loading -> Ready/Error states to manage the initial boot sequence.
- **Redirect Logic**: If no token or invalid session, the game redirects to the Login scene.

## 2026-05-21 - API Error Standardization & UTC Compliance
- **ErrorResponseHelper**: Centralized JSON error generation for consistent 401/503 responses across all middlewares.
- **UTC Enforcement**: Used `DateTime.SpecifyKind(..., DateTimeKind.Utc)` before serialization to ensure Godot correctly identifies timestamps as UTC.
- **Middleware Security**: Hardened whitelisting in `SessionAuthMiddleware` to prevent unauthorized access to internal endpoints.

## 2026-05-20 - API Performance & Eager Loading Strategy
- **Targeted Loading**: Removed blanket `.Include(p => p.GameStats)` from frequent auth paths to reduce payload size and DB load.
- **Eager Loading**: Used `.Include()` only on the profile endpoint where the full session history is actually required.
- **503 Standardization**: The `ExceptionHandlingMiddleware` now returns a structured JSON error response compatible with the Blazor frontend's error banners.

## 2026-05-24 - Profile Synchronization & Event-Driven Initialization
- **DTO to Domain Mapping**: Introduced `ProfileMapper` in Core to safely convert network POCOs into domain aggregates.
- **Event-Driven Architecture**: Used `ProfileLoadedEvent` to trigger inventory and stat initialization across decoupled Godot systems.
- **Idempotent Initialization**: Hardened `InventoryManager` to allow multiple initializations (e.g., Logout/Login) without data duplication.

## 2026-05-24 - Namespace Management & Godot Conflict Resolution
- **Namespace Conflict**: Resolved naming collision between `Core.Domain.Player` and `IslandSurvivor.Scenes.Player.Player`. Established standard of using fully qualified names for the domain model in Godot scripts.

## 2026-05-10 - US 20.0.4 : Architecture de l'Écran de Chargement et UX de Synchronisation

### Découvertes Architecturales
- **Gestion du ProcessMode au Démarrage** : Pour bloquer efficacement les entrées avant le chargement de la première scène de gameplay, le `GameManager` (Autoload) doit appliquer `ProcessModeEnum.Disabled` sur la `CurrentScene` du `SceneTree`.
- **Découplage UI/Logique via Signaux** : L'utilisation de signaux personnalisés (`RetryRequested`, `OfflineModeRequested`) dans `LoadingScreen.cs` permet d'éviter un couplage fort avec le `GameManager`, facilitant la maintenance et les tests.
- **Blocage des Inputs via CanvasLayer** : Un `CanvasLayer` avec une couche élevée (e.g., 128) et un `ColorRect` ayant `MouseFilter = Stop` est la méthode la plus robuste pour intercepter tous les événements d'entrée dans Godot 4.

### Quirks Godot/C#
- **Rotation Procédurale** : La rotation du spinner dans `_Process` doit utiliser `delta` pour assurer une fluidité constante indépendamment du framerate.
- **Transition de Scène et ProcessMode** : Lors de l'appel à `ChangeSceneToFile`, la nouvelle scène est chargée avec son propre `ProcessMode` (généralement `Inherit`), ce qui réactive implicitement le traitement du jeu après la disparition de l'écran de chargement.
