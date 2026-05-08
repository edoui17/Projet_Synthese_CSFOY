
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

## 2024-05-18 - Melee Hitbox Implementation
- **Architectural Shift:** For enemy melee attacks, instead of using continuous overlap checks via `GetOverlappingBodies()`, a dedicated `HitboxArea` (`Area2D`) handles collision tracking. The `BodyEntered` signal ensures that damage is applied instantly (once) upon contact when the player enters the area.
- **Quirk/Discovery:** It's important to configure the Godot Collision Masks accurately. To prevent enemies hitting themselves or other entities, the `HitboxArea` mask is set explicitly to the Player layer (Layer 3), and the logic enforces that the target is in the 'Player' group and implements `IDamageable`.

## Combat System Update & Architecture

-   **Godot Quirks:** When dynamically switching states from an Area2D overlap check (like a melee `HitboxArea`), it's safer to maintain a `HashSet<IDamageable>` of currently overlapping targets using `BodyEntered` and `BodyExited` rather than relying solely on `GetOverlappingBodies()`. This allows a continuously updated reference to targets, especially useful when an attack has a duration and we need to verify if the target is still in range during the attack frame.
-   **Architecture:** Formalized `IAgressorController` in `Src/Core/Interfaces/Entities/` to define the state machine API for aggressive NPCs. By moving attack duration and cooldown tracking into the core controller, the Godot client (`Soldier.cs`) simply queries `CanAttack()` and `StartAttack()`, mapping these logical states to visual animations (`m_animatedSprite.Play("Attack")`) without polluting the Godot script with timer logic.
