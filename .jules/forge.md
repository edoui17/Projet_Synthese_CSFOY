
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

1. **Create the Scene:**
   - Create a new Scene with a `CharacterBody2D` as the Root Node.
   - Rename the root node to `Sheep`.
   - Save the scene as `Sheep.tscn` in `Src/IslandSurvivor/Nodes/Entities/` (or your preferred Scenes folder).

2. **Add Child Nodes:**
   - Add a `Sprite2D` node. Assign a sheep texture to it.
   - Add a `CollisionShape2D` node. Assign a shape (like a `CapsuleShape2D` or `CircleShape2D`) that fits the sprite.
   - Add a `NavigationAgent2D` node. This is required by `Sheep.cs` (although currently for fleeing we use vector math + MoveAndSlide, having the node prepares for future pathfinding integrations and avoids errors).

3. **Attach the Script:**
   - Select the `Sheep` root node.
   - Attach the `Src/IslandSurvivor/Nodes/Entities/Sheep.cs` script to it.

4. **Configure Export Variables (Inspector):**
   - Click on the `Sheep` node. In the Inspector, under the `Sheep` script section:
     - `NpcType`: Leave as "Passive".
     - `IdleSpeed`: Adjust as desired (default is 30.0).
     - `FleeSpeed`: Adjust as desired (default is 120.0).
     - `MaxHealth`: Set the sheep's HP (default is 3).

5. **Collision & Layers:**
   - Make sure the `CharacterBody2D` is set to the correct Collision Layer (e.g., an "Enemy/NPC" layer) and masks the "World" layer so it collides with trees and rocks during `MoveAndSlide()`.
   - Ensure your Player's weapon/attack logic can detect this layer and call the `TakeDamage(int amount, object attacker)` method on the Sheep when hitting it.

6. **Signals (Inventory):**
   - No Godot GUI signals need to be manually connected for the inventory.
   - The C# script automatically emits the `SignalManager.Instance.EmitMaterialDestroyed(...)` event upon death.
   - Ensure the global `InventoryNode` and `SignalManager` AutoLoads are running in your project so the inventory receives the "Meat" resource.

### Technical Quirks Addressed
- **Enums Avoided:** Used static string constants (`SheepStates`) instead of enums.
- **Interfaces First:** Created `INpc`, `IDamageable`, and `IHealthComponent` before implementation.
- **Decoupled Logic:** The Flee calculations and Timers run in pure C# (`SheepController`) without relying on the Godot `_Process` delta directly inside the node (the node just passes the delta down).
### Integrating `StatManager` with the Godot Scene Tree
To meet the requirement to use the N-Tier statistics system, `Sheep.cs` exposes an `[Export] public StatManager Stats { get; set; }` property. Like `Rock.cs`, it requires the user to instantiate the Godot `StatsManager.tscn` as a child node in `Sheep.tscn`, and assign it via the inspector. We also created `SheepStats.tres` which holds the baseline values (`MaxHealth = 3.0`) for the entity so it seamlessly fits into the global Upgrade and Saving mechanics.
=======
### 2026-04-12 - Navigation System Architecture Documentation
- **Tags**: Navigation, Système, Architecture
- **Decision**: Created an exhaustive End-to-End documentation for the Navigation System (`WikiCode/SystemeNavigation.md`).
- **Learning**: The Navigation System successfully demonstrates the N-Tier architecture and Bridge pattern, ensuring Core logic (`NavigationService`) generates destinations and calculates costs abstractly. Interaction via Godot Nodes (`NavigationMenu`, `PortalInteraction`) triggers WeakEvents which are intercepted by Godot Singletons (`NavigationManager`) to persist state before deferring transition to `SceneLoadingManager`. This ensures stable decoupling between game state persistence and scene lifecycles.

- **Godot Save Migration & Paths:** Godot's `res://` maps directly to `Src/IslandSurvivor/`. To resolve a path back to the solution root programmatically within Godot, `ProjectSettings.GlobalizePath("res://../../Save/")` successfully breaks out of the Godot project bounds, pointing directly alongside the `.sln`. Using `DirAccess` and `FileAccess` is necessary to gracefully interact with these paths over raw `System.IO` to respect virtual directories (`user://`).

### $(date +'%Y-%m-%d') - Inheritance in Godot
- Discovered and addressed that when refactoring existing independent `.tscn` files to utilize inherited scenes, the child `.tscn` needs specific index pathing to safely inject elements (like Map scenes inside the inherited `MapContainer` node) over relying on the standalone architecture.
