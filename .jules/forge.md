# IslandSurvivor - Technical Log

## Sheep Implementation (US 4.4) - Godot Setup Instructions

For the implementation of the Sheep (Passive NPC), the core logic has been written in C# following the N-Tier architecture (`HealthComponent`, `SheepController` in `Core`, and `Sheep.cs` in `IslandSurvivor`).

### How to configure `Sheep.tscn` in Godot:

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