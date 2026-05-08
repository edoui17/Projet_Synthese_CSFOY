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

## 2024-05-18 - Signal-Based Attack Logic vs Area Polling
- **Quirk/Discovery:** In Godot, when activating a `CollisionShape2D` hitbox mid-animation via `AnimationPlayer` (e.g., turning `disabled` off at 0.2s), polling for overlapping areas manually in the same C# function call using `GetOverlappingAreas()` will fail if called instantly.
  - Using `await ToSignal(GetTree().CreateTimer(0.25f), SceneTreeTimer.SignalName.Timeout)` and then `GetOverlappingAreas()` works but can feel brittle.
  - The more idiomatic Godot solution is relying on the signals `AreaEntered` and `BodyEntered` emitted natively by the `Area2D` when the `disabled` flag flips to `false` during the animation frame.
- **Architectural Shift:** Moving from a procedural execution list to an event-driven `HashSet<IDamageable>` tracking mechanism ensures single-hits per target per attack frame while leveraging Godot's built-in physics event queue.

## 2024-05-20 - API & DB Audit
- **Security Discovery:** Plain text password storage is temporarily accepted for development validation, but the architecture is ready for BCrypt integration via `IAuthRepository`.
- **Architectural Shift:** Introduced `AuthResponse` and `ProfileResponse` DTOs in the Core layer. This ensures that Database Entities (Infrastructure) never leak into the API responses, maintaining a strict N-Tier separation and preventing accidental exposure of sensitive fields like `PasswordHash`.
- **Database Quirk:** EF Core `HasIndex(e => e.Username).IsUnique()` is essential even if the database has a `UNIQUE` constraint, as it allows EF to optimize queries and handle validation at the tracking level.
