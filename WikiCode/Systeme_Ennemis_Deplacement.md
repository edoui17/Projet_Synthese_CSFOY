# Navigation and Movement System for Enemies

This documentation explains how the navigation and movement systems were implemented for passive and aggressive enemies in the IslandSurvivor game (US 6.1).

## 1. Updated Navigation Approach (No NavMesh)

Following recent architectural decisions, we have simplified the enemy movement by removing `NavigationAgent2D` and `NavigationRegion2D`. The enemies now rely entirely on Godot's built-in physics engine (`MoveAndSlide`) and collision detection (`GetSlideCollisionCount()`) to navigate the world and avoid getting stuck against walls.

## 2. Setting up the Enemy Scene (`Soldier.tscn`)

Enemies require specific child nodes to utilize the C# scripts via the composition pattern.

### Step-by-Step Godot Node Setup
1. **Root Node**: Create a new scene with `CharacterBody2D` as the root. Name it `Soldier`.
2. **Animation**: Add a `Sprite2D` child and an `AnimationPlayer` child.
   - Configure the animations (e.g., `Idle`, `Moving`) in the `AnimationPlayer` to manipulate the `Sprite2D` frame properties.
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
- **Node-based StateMachine**: State management (IDLE, WANDER, CHASE) is handled by the Godot nodes (`StateMachine.cs` and `State.cs` derivatives). This replaces monolithic controller scripts like the deprecated `AgressorController`.
- `Soldier.cs` (or `AggressiveNpcBase`): The client-side Godot script that glues components. It manages target detection via Godot Groups ("Player") and relays information to the `StateMachine`.

For more details on the State Machine, consult [`NPC_StateMachine_Architecture.md`](./NPC_StateMachine_Architecture.md).

## 4. Summary of Accomplishments (US 6.1)
- Implemented Wandering (Idle Wandering) using `WanderState`, which captures a spawn position and assigns a random target within a radius.
- Implemented Obstacle Avoidance within the `WanderState` by checking `GetSlideCollisionCount() > 0` and handling collision internally (e.g., assigning a new random direction) to prevent state thrashing.
- Implemented Chase logic via `ChaseState` when the player is within the detection radius.
- Standardized animations using `Sprite2D` and `AnimationPlayer`, where the `MovementController` is responsible for flipping the `Sprite2D` (`FlipH`) based on the X direction.
