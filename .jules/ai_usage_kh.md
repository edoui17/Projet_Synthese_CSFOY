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
