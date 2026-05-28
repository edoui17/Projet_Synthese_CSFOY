# Navigation and Movement System for Enemies

This documentation explains how the navigation and movement systems were implemented for passive and aggressive enemies in the IslandSurvivor game (US 6.1).

## 1. Updated Navigation Approach (No NavMesh)

Following recent architectural decisions, we have simplified the enemy movement by removing `NavigationAgent2D` and `NavigationRegion2D`. The enemies now rely entirely on Godot's built-in physics engine (`MoveAndSlide`) and collision detection (`GetSlideCollisionCount()`) to navigate the world and avoid getting stuck against walls.

## 2. Setting up the Enemy Scene (`Soldier.tscn`)

Enemies require specific child nodes to utilize the C# scripts via the composition pattern.

### Step-by-Step Godot Node Setup
1. **Root Node**: Create a new scene with `CharacterBody2D` as the root. Name it `Soldier`.
2. **Animation**: Add a `Sprite2D` and `AnimationPlayer` child.
   - Create a new `SpriteFrames` resource.
   - Add two animations exactly named: `Idle` and `Moving`.
   - Assign the corresponding frames for your enemy in each animation.
3. **Collision**: Add a `CollisionShape2D` child. Define a shape (like a `CapsuleShape2D` or `CircleShape2D`) that matches the sprite's base. Set the collision layer to the "Enemy" layer and mask the "World" layer (so the enemy collides with walls).
4. **Movement**: Add a `MovementController` node by instantiating `MovementController.tscn` as a child.
5. **Stats**: Instantiate `StatsManager.tscn` as a child node.
6. **State Machine**: Add a generic `Node` and name it `StateMachine`. Instantiate state scripts as its children (e.g., `IdleState`, `WanderState`, `ChaseState`).

### Script Configuration
1. Attach `Soldier.cs` (or another appropriate derived class from `AggressiveNpcBase`) to the root `CharacterBody2D` node.
2. In the Inspector, assign the `StatManager` node to the `Stats` exported property.
3. In the individual State nodes (`ChaseState`, `IdleState`), configure exported properties like speeds, detection radii, and animation names.

## 3. Architecture

The C# logic was designed following the Orchestrator/Accountant N-Tier architecture:
- `IEnemy.cs` / `INpc.cs`: Interfaces defining base enemy types in Core.
- `StateMachine.cs`: Pure C# logic in `Logic/Entities/` to manage state changes (IDLE, CHASE, DEAD) and direction logic. It does not depend on Godot nodes directly.
- `Soldier.cs`: The client-side Godot script. It links the Godot components (`Sprite2D` / `AnimationPlayer`, `MovementController`) with the C# logic controllers. It manages target detection via Godot Groups ("Player"), physical movement via `MovementController.Move()`, and animation playback based on velocity.

## 4. Summary of Accomplishments (US 6.1)
- Implemented Wandering (Idle Wandering) using timers and random directions.
- Implemented Obstacle Avoidance by checking `GetSlideCollisionCount() > 0` during the `IDLE` state, immediately forcing the controller to pick a new random direction to prevent getting stuck.
- Implemented Chase logic when the player is within the detection radius.
- Replaced static sprites with `Sprite2D` and `AnimationPlayer`, ensuring the enemy flips visually based on the X direction and switches between `Idle` and `Moving` animations.
