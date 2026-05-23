# AI Usage Journal

### 2026-03-22 - [User Story 2.2 : Implémenter l'interaction du joueur]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **User Story 2.2: Implémenter l'interaction du joueur.**<br>- Implement detection, visual indicator, interaction triggering, and prioritization logic (closest object).<br>- **Tasks**: Configure Input, Create Movement Data, Physics Movement, Player Collision, Environment Collision, Validation, Documentation. | **1. Core Logic**: Created `IInteractable` (`Src/Core/Interfaces/`) and `InteractionService` (`Src/Core/Managers/`) to handle agnostically the prioritization of the closest object based on coordinates.<br>**2. Player State**: Defined `PlayerState` enum (`Src/Core/Domain/`) to manage Idle, Moving, and Interacting states via a state machine.<br>**3. Movement Data**: Implemented `PlayerMovementData` as a Godot `Resource` to store acceleration and friction, following the project's data-driven approach.<br>**4. Godot Player**: Refactored `Player.cs` and `Player.tscn` to use `CharacterBody2D`, integrate with `StatManager` (for Speed), and implement the state machine. Added a "Press E" `Label` for visual feedback.<br>**5. Interaction Bridge**: Created `InteractableNode.cs` inheriting from `Area2D` and implementing `IInteractable`, allowing Godot nodes to easily participate in the Core's interaction logic.<br>**6. Scene Cleanup**: Replaced the legacy player scene in `BaseMap` with the standardized version to ensure system consistency. | **N-Tier Separation**: By moving the "brain" (sorting by distance) to the Core's `InteractionService`, we maintain a clean boundary between engine physics (detection) and business logic (selection).<br>**Physics-Based Locomotion**: Switching the player to `CharacterBody2D` and using `MoveAndSlide` with acceleration/friction provides a more responsive and professional feel compared to simple position increments.<br>**Extensibility**: The `InteractableNode` bridge makes it trivial to add new interactable types (NPCs, chests, etc.) by simply inheriting and overriding the `Interact()` method, while automatically benefiting from the proximity detection system. |

### 2026-03-22 - [Task 2.2.5 : Implémenter Attaque du Joueur]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Task 2.2.5: Implémenter Attaque du Joueur.**<br>- Implement attack triggering (Space key), movement blocking, and hit detection. | **1. Interfaces**: Defined `IAttackable` interface in `Src/IslandSurvivor/Interfaces/`.<br>**2. Attackable Node**: Created `AttackableNode` in `Src/IslandSurvivor/Nodes/` as a base for enemies/objects that can be hit.<br>**3. Player Refactoring**: Updated `Player.cs` to handle the `Attacking` state, play `ATTACK` animation, and block movement. Integrated `m_weaponArea` for hit detection.<br>**4. Tscn Update**: Linked `WeaponInteractionArea` in `Player.tscn`.<br>**5. Verification**: Added unit test for `IAttackable` interface. | **Separation of Concerns**: Kept combat separate from the general interaction system. Used the existing `PlayerState.Attacking` and `AnimationPlayer` frames to enable/disable the weapon hitbox, ensuring precise hit registration. |

### 2026-04-19 - [Persistence] N-Tier Database Schema & EF Core

**Request**: Create database tables and relationships for Inventory, Stats, and PlayerConfig meta-progression. Provide SQL DDL and EF Core implementation.
**AI Contribution**:
1. Created SQL DDL script `schema.sql` with tables for `Players`, `ResourceItems`, `Inventory`, `Stats`, and `PlayerConfig`.
2. Refactored domain models (`Player`, `InventoryEntry`, etc.) into `Src/Core/Domain` to satisfy N-Tier architecture.
3. Defined `IRepository<T>` interface in `Src/Core/Interfaces` following "Interfaces First" rule.
4. Implemented `AppDbContext` in `Src/Infrastructure` mapping Core models via Fluent API.
5. Translated and updated persistence documentation to English (`WikiCode/Persistence_System.md`).
**Decision Reasoning**: Adhering to strict N-Tier and "Interfaces First" requirements ensures long-term maintainability and clean separation of concerns. Moving entities to the Core Domain allows all layers (API, Godot, Infrastructure) to share the same business models without cyclic dependencies.

### 2026-04-20 - [US 7.1.1] | Implementation of DAL and SQL Schema | Defined persistence scope as Meta-Progression and instructed on EF Core integration within Infrastructure layer.

**AI Contribution**:
1. Updated SQL schema with `ExtraStats` column for flexibility.
2. Implemented repository interfaces in Core and implementations in Infrastructure.
3. Mapped Infrastructure entities to Core domain models within the repositories.
4. Documented the use of `ExtraStats` for future-proofing stats.
**Decision Reasoning**: Column-per-stat optimizes reads for core meta-progression, while a JSON column provides the necessary flexibility for future attributes without schema overhead.

### 2026-04-21 - [Cleanup] | Removal of Resolution and IsFullScreen from PlayerConfig | Removed unnecessary fields from PlayerConfig throughout the N-Tier architecture.

**AI Contribution**:
1. Removed fields from `Core.Domain.PlayerConfig` and `Infrastructure.Entities.PlayerConfigEntity`.
2. Updated `AppDbContext` and `PlayerRepository` to eliminate dependencies on these fields.
3. Updated SQL schema script `schema.sql` and `Persistence_System.md` documentation.
4. Initialized and added Entity Framework Core migration `InitialCreate` for the Infrastructure project.
**Decision Reasoning**: Simplifies the persistence model by removing fields not required for meta-progression. Initializing EF migrations ensures the development environment can correctly synchronize the database state with the updated models.

### 2026-04-21 - [US 7.1.2] | Creation of initial configuration data script | Created a DML script to populate the database with default configuration and test data.

**AI Contribution**:
1. Created `Src/Infrastructure/database/script/data.sql` with seed data for all persistence tables.
2. Configured default `Speed` (vitesse de base) to 1.0 for test players.
3. Populated `ResourceItems` with game-matching entities (Wood, Rock, Gold, Meat).
4. Updated `WikiCode/Persistence_System.md` to document the new script.
**Decision Reasoning**: Providing a pre-configured data script allows for immediate testing of the meta-progression system and ensures consistent base configuration (like speed) across different environments.
### 2026-04-22 - [User Story 8.1 : Synchronisation des données de progression]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 8.1: Synchronisation des données.**<br>- Implement API endpoints for profile loading and data sync.<br>- Implement authentication with username/password and session tokens.<br>- Handle stats, inventory, and configuration. | **1. Database Evolution**: Added `PasswordHash` and `SessionToken` to `PlayerEntity` and migrated the database.<br>**2. Core Domain**: Created `PlayerProfile`, `LoginRequest`, and `SyncRequest` POCOs for optimized data transfer.<br>**3. Repositories**: Implemented `AuthRepository` and updated `PlayerRepository` to handle the new security fields and manual mapping.<br>**4. API Layer**: Developed `AuthController` for session management and `PlayerController` for consolidated profile/sync operations. Added `StatsController` and `InventoryController` for granular updates. | **Security & UX**: The session token system balances security with ease of use for the game client. The consolidated profile endpoint minimizes startup latency, while individual UPSERTs allow for frequent, low-overhead saving during gameplay. |

### 2026-05-08 - [Audit] | Audit Technique et Validation (API & DB)

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Audit Technique et Validation.**<br>- Revue du schéma DB et intégrité.<br>- Audit des contrôleurs API et DTOs.<br>- Simulation Postman. | **1. DB Integrity**: Added `UNIQUE` constraint to `Username` and synchronized `schema.sql` with authentication fields.<br>**2. N-Tier Isolation**: Created `AuthResponse` and `ProfileResponse` DTOs to replace direct entity/domain model exposure in API.<br>**3. Controller Refactoring**: Updated `AuthController` and `PlayerController` to use DTOs and fixed the leaderboard endpoint.<br>**4. Documentation**: Generated a full Technical Audit Dashboard with Postman simulations. | **Strict N-Tier Compliance**: Ensuring DTOs are used for all API outputs prevents leaking sensitive infrastructure details (like password hashes) and maintains decoupling. Unique constraints at both SQL and EF level provide defense-in-depth for data integrity. |

### 2026-05-09 - [Audit] | Audit Technique, Sécurité et Résilience (POCO & CRUD)

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Audit Technique, Sécurité et Résilience.**<br>- Suppression de doublons.<br>- Mise à jour des dates (09/05/2026).<br>- Simplification Auth.<br>- Résilience Sync (503).<br>- Simulation CRUD & Cybersécurité. | **1. Maintenance**: Deleted `jules/forge.md` and updated all log dates to May 9, 2026.<br>**2. Resilience**: Implemented try-catch in `Sync` endpoint to handle DB failures with 503 status. Documented client fallback strategy.<br>**3. Security Audit**: Documented SQL Injection protection by EF Core and simulated Brute Force handling.<br>**4. CRUD Validation**: Documented full CRUD flows for Player, Stats, and Inventory using POCOs. | **Resilience First**: In a Rogue-like, losing progress due to a DB glitch is unacceptable. The 503/Fallback strategy ensures the client remains the secondary source of truth. Moving to POCO terminology aligns with pure C# data structures used for transport. |

### 2026-05-09 - [Infrastructure] | Optimisation de la Résilience et Sécurisation

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Optimisation de la Résilience et Sécurisation.**<br>- EnableRetryOnFailure.<br>- TrustServerCertificate=True.<br>- Middleware X-API-KEY. | **1. DB Resilience**: Configured EF Core with `EnableRetryOnFailure` and updated connection string with `TrustServerCertificate=True` to handle transient connection issues.<br>**2. API Security**: Implemented `ApiKeyMiddleware` to enforce `X-API-KEY` validation on all endpoints.<br>**3. Documentation**: Updated `Manual_Testing_Guide.md` with API key requirements for Postman. | **Defense in Depth**: API Key provides an immediate security layer while full JWT auth is being developed. `EnableRetryOnFailure` is standard practice for cloud-ready SQL connections to improve robustness against network blips. |

### 2026-05-09 - [Infrastructure] | Infrastructure, schéma SQL et corrections d'environnement

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Infrastructure, schéma SQL et corrections.**<br>- Schema DB (Reset).<br>- Mapping SQL REAL.<br>- SQL Developer Instance.<br>- Visual Studio Env. | **1. Schema Strategy**: Updated `schema.sql` with a full DB reset header (master/drop/create) for environment purity.<br>**2. Type Safety**: Replaced `FLOAT` with `REAL` in the schema to match C# 32-bit floats and eliminate casting errors.<br>**3. Connection String**: Updated `appsettings.json` for SQL Developer instance and SSL trust.<br>**4. Memory**: Recorded the new environment standards (VS Full, REAL types). | **Operational Stability**: Standardizing the DB reset and the exact type mapping (REAL/float) eliminates common "InvalidCastException" and environment drift issues during manual testing. |

### 2026-05-09 - [Infrastructure] | Standards de déploiement et cycle de vie de la base de données

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Mise à jour des standards de déploiement.**<br>- Schema.sql (Robustesse DROP TABLE).<br>- Guide configuration (Server=). | **1. Schema Lifecycle**: Added `IF OBJECT_ID DROP TABLE` to `schema.sql` to ensure error-free re-initialization despite constraints.<br>**2. Developer UX**: Updated `Manual_Testing_Guide.md` to instruct manual `Server=` configuration in `appsettings.json` per local environment requirements. | **Zero Friction**: Automating the cleanup of existing objects in the SQL script prevents metadata conflicts, while explicitly documenting local connection requirements avoids environment-specific connectivity blockers for the team. |
| 2026-05-09 | **Refactorisation de la Configuration et Nettoyage.**<br>- Consolidation API Key.<br>- Suppression code mort PlayerController. | **1. API Security**: Removed hardcoded `API_KEY` from `ApiService.cs`. Updated constructor to inject the key from configuration.<br>**2. Client Config**: Moved the API Key to `project.godot` (Godot) and `appsettings.json` (Web).<br>**3. Code Cleanup**: Deleted duplicate/commented-out leaderboard endpoint in `PlayerController.cs`. | **Architectural Purity**: Moving secrets to configuration files adheres to "Separation of Concerns" and "Security by Design". Removing dead code reduces cognitive load and prevents maintenance errors. |

### 2026-05-14 - [US 18.0] | Refonte de la persistance et logique de HighScore | Mise à jour du schéma DB pour supporter l'historique des sessions et le calcul automatique du HighScore via Trigger.

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 18.0: Visualisation des statistiques.**<br>- Migration de `Stats` vers `GameStats` (1-N).<br>- Ajout de `HighScore` à `Players`.<br>- Implémentation du Trigger `TR_GameStats_AfterInsert`.<br>- Mapping `Duration` vers `TimeSpan`. | **1. Core Domain**: Renamed `PlayerStats` to `GameStats`, added session metrics (Score, Duration, LevelReached, BonusStats). Updated `Player` for 1-N relationship.<br>**2. Infrastructure**: Updated `AppDbContext` and entities. Created a simplified EF Core migration for a 'Clean Slate' deployment.<br>**3. SQL Logic**: Embedded the `TR_GameStats_AfterInsert` trigger in the migration to automate HighScore updates and prune history to top 10.<br>**4. API Layer**: Updated `StatsController` and `PlayerController` for session-based tracking and profile history. | **Performance & Scalability**: Offloading the HighScore calculation and history pruning to a database trigger ensures data consistency regardless of the API's state. A 'Clean Slate' approach ensures environment purity for the competitive dimension. |

### 2026-05-14 - [Task 18.0.2] | Refactorisation de l'API et logique de calcul | Implémentation du ProgressionService et optimisation du leaderboard.

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Task 18.0.2: Refactorisation API.**<br>- Extraire la logique métier (niveaux, records).<br>- Créer `ProgressionService` (Core).<br>- Optimiser le leaderboard (HighScore Players).<br>- Assurer la cohérence mémoire/réponse. | **1. Core Logic**: Created `IProgressionService` and `ProgressionService` with threshold-based leveling (0-999, 1000-2499, 2500-4999, +5000) and high score update logic.<br>**2. Controller Refactoring**: Injected `IProgressionService` into `PlayerController` and `StatsController`. Updated `Sync` and `AddSession` to use the service for calculations before DB persistence.<br>**3. Leaderboard Optimization**: Modified the leaderboard query to order directly by the `HighScore` column in the `Players` table, improving performance.<br>**4. Consistency**: Guaranteed immediate API response consistency by updating the `Player` object in memory before the 200 OK response. | **Separation of Concerns**: Moving progression math to a dedicated service in Core protects the API layer from logic drift and centralizes balancing. **Performance**: Leveraging the direct `HighScore` column for leaderboards avoids expensive aggregate calculations across millions of session records. |

### 2026-05-15 - [US 10.1.5] | Extension de l'endpoint Leaderboard pour le filtrage et le tri

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 10.1.5: Implémentation du filtrage.**<br>- Modifier `GetLeaderboard` pour accepter `sortBy`, `order`, `search`.<br>- Gérer le tri par Score, Temps de jeu, Niveau, et Stats Bonus.<br>- Enrichir le POCO `PlayerLeaderboardEntry`. | **1. Core Domain**: Added `Duration` and `BonusStats` fields to `PlayerLeaderboardEntry` for frontend flexibility.<br>**2. API Layer**: Refactored `PlayerController.GetLeaderboard` with LINQ-based sorting and filtering.<br>**3. Search**: Implemented case-insensitive search for usernames.<br>**4. Projection**: Ensured data consistency by projecting the "Best Session" (highest score) into the POCO for all sorting types. | **Frontend Empowerment**: Providing all bonus stats and duration in the POCO allows the web dashboard to highlight the relevant metric without extra API calls. **Architectural Consistency**: Sorting by `Max()` of a stat while displaying the "Best Session" maintains a high-quality "Hall of Fame" where only the absolute best performances are showcased. |

### 2026-05-18 - [US 17.0.1 : Sécurisation, Routage et État Global du Dashboard]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 17.0.1: Sécurisation et Routage.**<br>- Persistance du Jeton (LocalStorage).<br>- Contrôle d'accès (Guard / Redirection).<br>- Gestionnaire d'API Global (Header injection). | **1. Auth Infrastructure**: Implemented `CustomAuthenticationStateProvider` and `AuthService` using native `IJSRuntime` for LocalStorage persistence.<br>**2. Global API Handler**: Created `SessionTokenHandler` to automatically inject `x-Session-Token` and handle `401 Unauthorized`.<br>**3. UI Protection**: Configured `Routes.razor` with `AuthorizeRouteView` and implemented component-level navigation guards in `Login.razor` and `Dashboard.razor`. | **Security by Default**: Centralizing token injection in a `DelegatingHandler` ensures that all outgoing requests are authenticated without manual boilerplate. **User Experience**: Implementing bi-directional guards (redirect to login if unauth, redirect to dashboard if auth) provides a seamless and secure navigation flow. |

### 2026-05-21 - [Task 17.0.3 : Intégration de l'Interface du Dashboard et Consommation du Profil]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Task 17.0.3: Dashboard Integration.**<br>- Consume `/api/player/profile` API.<br>- Implement loading spinner and error banners (503).<br>- Bind data to UI (Stats, History, Audit). | **1. UI Logic**: Implemented async data fetching in `OnInitializedAsync`. Added `m_isLoading` and `m_errorMessage` states.<br>**2. Resilience**: Wrapped API calls in try-catch blocks with 503-specific logging and user-friendly error banners.<br>**3. Data Display**: Integrated `ProfileResponse` data with Bootstrap 5 components. Implemented "Best Session" logic by projecting the first element of `LastSessions`.<br>**4. Audit**: Displayed the `UpdatedAt` field in `dd/MM/yyyy HH:mm` format for user audit. | **UX Continuity**: Ensuring the dashboard remains interactive (with loading and error states) prevents users from thinking the application has crashed during API outages. **Data Integrity**: Using the "Best Session" projection ensures visual consistency with the leaderboard, reinforcing the player's meta-progression achievements. |

### 2026-05-24 - [US 20.0.1 : Synchronisation BD vers le Jeu (Stats / Personnage)]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 20.0.1: Gestion de Session, Initialisation et Accès Jeu.**<br>- Persistance du Jeton (user://).<br>- GameManager (Machine à états).<br>- Scène de Login dédiée.<br>- Pop-up d'erreur et Mode Hors Ligne. | **1. Core Evolution**: Added `AppStatus` enum and enriched `IApiService` with token management.<br>**2. Persistence**: Created `SessionProvider` using Godot's `ConfigFile` for OS-agnostic session storage in `user://`.<br>**3. Orchestration**: Implemented `GameManager` (Autoload) to handle the startup sequence (Validation -> Redirect).<br>**4. UI/UX**: Created `Login.tscn` and `ErrorPopup.tscn` to handle authentication and API connection failures. | **Separation of Responsibilities**: Decoupling service initialization (`ServiceRegistry`) from the application lifecycle (`GameManager`) ensures a cleaner boot sequence. **Deployment Readiness**: Using `user://` is the only way to guarantee persistent data storage across all platforms when the game is exported. |

### 2026-05-21 - [US 20.0.2 : Point d'accès de Synchronisation et Validation Backend]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 20.0.2: Consolidation et Sécurisation API.**<br>- Standardiser les erreurs 401 et 503.<br>- Forcer le format UTC (Z) pour UpdatedAt.<br>- Refactoriser la gestion des erreurs en Middleware. | **1. Centralization**: Created `ErrorResponseHelper` to eliminate duplication in error JSON generation.<br>**2. Middleware Refinement**: Updated `SessionAuthMiddleware` and `ApiKeyMiddleware` to use the helper and improved URL whitelisting.<br>**3. UTC Fix**: Injected `DateTime.SpecifyKind` in the profile endpoint to guarantee ISO 8601 compliance.<br>**4. Resilience**: Extended `ExceptionHandlingMiddleware` to return 503 for all database connectivity issues. | **Contractual Robustness**: Ensuring a standardized error format across all middlewares simplifies client-side error handling in Godot. **Time Synchronization**: Explicitly forcing the 'Z' suffix in JSON is the most reliable way to handle cross-platform time parsing between .NET and Godot's C++ core. |

### 2026-05-20 - [US 17.0.2 : Point d'accès de Synchronisation et Audit Backend]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 17.0.2: Audit et Robustesse Backend.**<br>- Ajouter `UpdatedAt` au profil.<br>- Optimiser le chargement des sessions (Eager Loading).<br>- Standardiser le JSON d'erreur 503. | **1. Core Domain**: Added `UpdatedAt` to `ProfileResponse` POCO for UI audit display.<br>**2. API Middleware**: Refactored `ExceptionHandlingMiddleware` to return a structured JSON (error, message, timestamp) on database failure.<br>**3. Infrastructure**: Optimized `PlayerRepository` and `AuthRepository` by removing full `GameStats` inclusion, favoring targeted fetching via `StatsRepository`.<br>**4. Resilience Test**: Validated 503 JSON response via connection string sabotage. | **Performance at Scale**: Avoiding full eager loading of collections that grow over time is critical for long-term API performance. **Standardized Error Handling**: Providing a parsable JSON error during infrastructure downtime ensures the Blazor frontend can display meaningful feedback to the player rather than a generic crash. |

### 2026-05-24 - [US 20.0.3] | Intégration Jeu et Mapping des Données | Consommation de l'API, mapping vers le domaine et initialisation du gameplay.

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 20.0.3: Integration Jeu / Mapping.**<br>- Refactoriser 'IApiService' pour 'ProfileResponse'.<br>- Créer 'ProfileMapper' (Core).<br>- Initialiser l'inventaire et les stats.<br>- Gérer le mode Hors Ligne via cache. | **1. Core Utils**: Created 'ProfileMapper' to decouple network DTOs ('ProfileResponse') from domain aggregates ('PlayerProfile'), handling null safety for collections.<br>**2. Infrastructure Refactoring**: Updated 'IApiService' and 'ApiService' to support cached profile retrieval and 'ProfileResponse' as the primary source of truth.<br>**3. Game Logic**: Enhanced 'IInventoryManager' and 'InventoryManager' with idempotent initialization logic and 'ProfileLoadedEvent' subscription.<br>**4. Godot Orchestration**: Integrated 'GameManager' startup flow to publish 'ProfileLoadedEvent', triggering automatic synchronization of local player stats and inventory with remote data. | **Decoupling & Robustness**: Using a mapper and an event-driven approach ensures that gameplay systems remain agnostic of the network layer. **Stability**: Null-safe mapping prevents runtime crashes from malformed JSON or empty caches, while interface-level access to the cache respects N-Tier constraints. |

### 2026-05-25 - [US 20.0.4] | Implémentation de l'écran de chargement, blocage des inputs et gestion de l'UI d'erreur dans Godot

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 20.0.4: Front Jeu / UI & UX.**<br>- Écran de chargement dédié (CanvasLayer).<br>- Sécurisation des entrées (ProcessMode / MouseFilter).<br>- Gestion dynamique des erreurs (401, 503). | **1. UI/UX Design**: Created a standalone `LoadingScreen` scene with a procedural spinner and error section. Configured `MouseFilter.Stop` to block all inputs.<br>**2. Engine Orchestration**: Updated `GameManager` to manage the loading screen lifecycle and toggle `ProcessMode.Disabled` on the current scene.<br>**3. Error Mapping**: Implemented context-specific error messages (Session expiry vs Server issues) within the loading flow.<br>**4. Decoupling**: Used signals (`RetryRequested`, `OfflineModeRequested`) to decouple the UI from the manager. | **Robust User Experience**: Blocking inputs and providing clear, localized feedback during network operations is critical for a polished feel. **Process Safety**: Disabling the background scene's process mode prevents edge-case logic from running before data is fully synchronized. |
### 2026-05-25 - [US 20.0.5 : Sécurité et Résilience]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 20.0.5: Sécurité / Validation / Backoff.**<br>- Cooldown sur le bouton Retry (Backoff linéaire).<br>- Validation des stats (Anti-cheat).<br>- Transition hors ligne (1.5s).<br>- Documentation infrastructure Auth. | **1. Core Utils**: Implemented `ProfileValidator` with thresholds for Health (0-100] and other stats [0-999) to prevent corrupted data injection.<br>**2. Godot UI**: Enhanced `LoadingScreen.cs` with `StartRetryCooldown` and a dynamic countdown timer for the Retry button.<br>**3. Game Logic**: Updated `GameManager.cs` to manage `m_retryAttempt`, perform profile validation, and handle the 1.5s transparent offline transition.<br>**4. Unit Testing**: Created `ProfileValidatorTests.cs` to validate all security thresholds.<br>**5. Documentation**: Appended architecture details for Authentication and SQL Server dependency to `jules/forges.md`. | **Robustness & Anti-Spam**: The linear backoff protects the API from request flooding during outages. **Data Integrity**: Validating profile data at the Core layer ensures that gameplay systems are never initialized with "impossible" values, serving as a first line of defense against local data tampering. |

### 2026-05-25 - [Task 11.0.1 : Interface Graphique du Formulaire et Validation Locale]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Task 11.0.1: Frontend / UI & UX.**<br>- Finaliser la scène de Login existante.<br>- Implémenter la validation locale dans Core.<br>- Gérer l'ergonomie (Tab, Entrée, Toggle Password). | **1. Core Logic**: Created `LoginValidator.cs` and associated xUnit tests to centralize validation rules (min 3 chars).<br>**2. UI Enrichissement**: Refactored `Login.tscn` with `CenterContainer`, `MarginContainer` and `VBoxContainer`. Added password toggle button and Error label.<br>**3. Scripting**: Updated `Login.cs` with `%UniqueNames` access, `OfflineModeRequested` signal, and Tab focus management. | **N-Tier Portability**: Moving validation to Core ensures consistency with the upcoming Blazor dashboard. **UX Polishing**: Supporting Tab and Enter is essential for a professional PC feel, while local validation prevents unnecessary network calls for invalid inputs. |

### 2026-05-23 - [Task 11.0.1 : Interface Graphique du Formulaire et Validation Locale]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Task 11.0.1: Frontend / UI & UX.**<br>- Restructurer la scène de Login (Src/IslandSurvivor/Scenes/UI/LoginScreen/).<br>- Renommer la classe en LoginScreen.<br>- Mettre à jour GameManager.<br>- Documenter le guide de test. | **1. Restructuration** : Déplacement et renommage de `Login.tscn`/`cs` vers `LoginScreen.tscn`/`cs`. Nettoyage du dossier obsolète.<br>**2. Code & UI** : Mise à jour du namespace, du nom de classe et de la référence du script dans la scène. Ajustement de la couleur du `ErrorLabel` (#ff5555).<br>**3. Orchestration** : Mise à jour du `GameManager.cs` pour pointer vers le nouveau chemin de scène (`res://Scenes/UI/LoginScreen/LoginScreen.tscn`).<br>**4. Documentation** : Ajout d'un guide de test complet (dépendances infra, scénarios, comptes de test) dans `forges.md`. | **Standardisation de l'Équipe** : Aligner la structure des dossiers sur les standards du projet facilite la collaboration et évite les conflits de fusion. **N-Tier Integrity** : Garantir que le GameManager orchestre correctement le flux d'authentification vers la nouvelle scène renforce la robustesse du cycle de vie de l'application. |

### 2026-05-23 - [Task 11.0.2 : Branchement API, Cycle de Vie du Token et Déconnexion]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **US 11.0.2: Backend Client / Câblage Logique & Routage.**<br>- Brancher l'UI sur `IApiService.LoginAsync`.<br>- Gérer la persistance du jeton (auto-login).<br>- Implémenter la déconnexion (Logout).<br>- Gérer les retours API (401, 503) avec feedback UX. | **1. UI Logic**: Implemented async login flow in `LoginScreen.cs` with neutral loading messages, input locking, and specialized red error messages. Redirected successful login to `LoadingScreen.tscn`.<br>**2. Persistence**: Verified and enhanced `GameManager.cs` to check for stored tokens at startup using `SessionProvider`. If valid, it proceeds to US 20.0 synchronization.<br>**3. Logout**: Added a "Se déconnecter" button to `MainMenu.tscn` and connected it to a new `Logout` method in `GameManager` that clears the token and redirects to login.<br>**4. Decoupling**: Refactored `SignalManager` to include an `OfflineModeRequested` signal, allowing the UI to trigger guest mode in the `GameManager` without direct coupling.<br>**5. Testing**: Created `ApiServiceAuthTests.cs` in `Tests/UnitTests/` using `Moq` to validate success, failure, and network error scenarios. | **UX & Resilience**: Locking the UI during async calls prevents duplicate requests. Providing French error messages for specific HTTP codes (401 vs 503) improves player guidance. **Architecture**: Using the `SignalManager` for the offline transition preserves the strict N-Tier separation between UI and global managers. **Flow Consistency**: Redirecting to the loading screen after login ensures that all meta-progression data (Inventory, Stats) is correctly synchronized before the player enters the main menu. |
