# Forge Instructions - Melee Combat Implementation (US 6.3)

Follow these step-by-step instructions to configure the Godot Editor for the Melee Combat system.

## Task 1: Integrate StatManager into Enemy
1. Open the `Soldier.tscn` scene in the Godot Editor.
2. In the Scene tree, instantiate or add a `StatManager` node as a child of the root `Soldier` node. (Name it exactly `StatManager`).
3. In the Inspector for the `Soldier` root node, assign this newly created `StatManager` node to the `Stats` exported variable.
4. On the `StatManager` node itself, ensure the base health is configured correctly (e.g., 10 HP).

## Task 2 & 3: Configure Melee Hitbox and Layers
1. Still in `Soldier.tscn`, add a new `Area2D` node as a child of the `Soldier` root node.
2. Name this node exactly `HitboxArea` (Case-sensitive, as it is referenced in C#).
3. Add a `CollisionShape2D` as a child of `HitboxArea`. Define its shape (e.g., a `CircleShape2D` or `RectangleShape2D`) and size it to represent the attack reach of the Soldier.
4. Configure the **Collision settings for `HitboxArea`**:
   - **Layer**: None (Uncheck all)
   - **Mask**: Check Layer 3 (Assuming Player is on Layer 3). This ensures the `HitboxArea` only detects the Player.
5. Make sure your main root node (`Soldier`, which is a `CharacterBody2D`) has its **Collision settings** configured properly to receive attacks:
   - **Layer**: Check Layer 5 (Enemies / Resources).
   - **Mask**: Check Layer 1 (World/Walls), Layer 3 (Player, if collision is desired), etc.
   - *Note: The player's weapon Area2D (e.g., `WeaponAreaRight` / `WeaponAreaLeft`) should have its Collision Mask set to Layer 5 to intersect with the Soldier.*

## Task 4: Death and Loot Handling
- No new Godot nodes needed. Ensure the AutoLoads (`SignalManager`, `ScoreManager`, `InventoryNode`) are properly configured in `Project -> Project Settings -> Autoload`, as the `Soldier.cs` script relies on them globally to award points and trigger loot upon death.
