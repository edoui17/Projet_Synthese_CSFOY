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
