# Navigation and Movement System for Enemies

This documentation explains how the navigation and movement systems were implemented for passive and aggressive enemies in the IslandSurvivor game (US 6.1).

## 1. Scene Setup for Navigation

For enemies to navigate around obstacles, the map must define navigable areas.

### Setting up `NavigationRegion2D`
1. Open your main map scene (e.g., `BaseMap.tscn` or `Level1.tscn`).
2. Add a `NavigationRegion2D` node as a child of the map root or tilemap.
3. In the Inspector for the `NavigationRegion2D`, assign a new `NavigationPolygon`.
4. Draw the polygon over the areas where you want enemies to be able to walk. Ensure the polygon avoids walls, water, or other obstacles.
5. If you are using TileMaps with built-in navigation layers, ensure your tileset has the navigation layer configured properly for walkable tiles.

## 2. Setting up the Enemy Scene (`Sheep.tscn` or `Soldier.tscn`)

Enemies require specific child nodes to utilize the C# scripts.

### Step-by-Step Godot Node Setup
1. **Root Node**: Create a new scene with `CharacterBody2D` as the root. Name it `Sheep` or `Soldier`.
2. **Sprite**: Add a `Sprite2D` child. Assign your texture (e.g., idle/walk animation frames).
3. **Collision**: Add a `CollisionShape2D` child. Define a shape (like a `CapsuleShape2D` or `CircleShape2D`) that matches the sprite's base. Set the collision layer to the "Enemy" layer and mask the "World" layer.
4. **Navigation**: Add a `NavigationAgent2D` child. This is absolutely required for the scripts to calculate paths around obstacles. Keep its default settings, but you can adjust `PathDesiredDistance` and `TargetDesiredDistance` in the script.
5. **Movement**: Add a `MovementController` node by instantiating `MovementController.tscn` as a child.
6. **Stats**: Instantiate `StatsManager.tscn` as a child node.

### Script Configuration
1. Attach `Sheep.cs` (or `Soldier.cs`) to the root `CharacterBody2D` node.
2. In the Inspector, assign the `StatManager` node to the `Stats` exported property.
3. Assign the base variables such as `IdleSpeed`, `FleeSpeed`, or `ChaseSpeed`.

## 3. Architecture

The C# logic was designed following the N-Tier architecture:
- `IEnemy.cs` / `INpc.cs`: Interfaces defining base enemy types in Core.
- `PassiveController.cs` & `AgressorController.cs`: Pure C# logic in `Logic/Entities/` to manage state changes (IDLE, FLEE, CHASE, DEAD) and direction logic. They do not depend on Godot nodes directly.
- `Sheep.cs` & `Soldier.cs`: The client-side Godot scripts. They link the Godot components (`NavigationAgent2D`, `MovementController`) with the C# logic controllers. They retrieve the next path position from the `NavigationAgent2D` and pass the normalized direction to the `MovementController.Move()` method.

## 4. Summary of Accomplishments (US 6.1)
- Implemented Wandering (Idle Wandering) using timers and random directions combined with `NavigationAgent2D`.
- Implemented Obstacle Avoidance by retrieving `GetNextPathPosition()` from the nav mesh.
- Standardized controllers into reusable components (`PassiveController` for fleeing animals, `AgressorController` for chasing enemies).
- Fully integrated with `MovementController` to leverage stat-based speed boosts and Godot's native `MoveAndSlide()`.