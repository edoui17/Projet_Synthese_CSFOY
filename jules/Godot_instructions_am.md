# Godot Setup Instructions

## Arrow Scene (Projectile)
1. **Create a new Scene** (`Arrow.tscn`) in `res://Scenes/Projectiles/`.
2. Add an `Area2D` as the root node and rename it to `Arrow`.
3. Attach the `Arrow.cs` script (`res://Scenes/Projectiles/Arrow.cs`).
4. Add a `CollisionShape2D` as a child and assign a `RectangleShape2D` (e.g., small rectangle).
5. Add a `Sprite2D` as a child and assign an arrow image. Make sure the arrow points to the Right (0 degrees) by default.
6. Configure `CollisionLayer` and `CollisionMask`:
   - It's a projectile. To hit the player, ensure `CollisionMask` includes Layer 3 (Player).
   - If players use it to hit enemies, include Layer 5 (Enemies).
7. Save the scene as `Arrow.tscn`.

## Archer Scene
1. Duplicate `Soldier.tscn` and rename it to `Archer.tscn`.
2. Change the attached script from `Soldier.cs` to `res://Scenes/NPC/Agressive/Archer.cs`.
3. In `Archer.tscn`, update the `DetectionArea` `CollisionShape2D` to have a larger radius than the Soldier (e.g., radius = 300) so it can detect the player earlier.
4. Set the `StoppingDistance` exported property to `200` in the Inspector.
5. Provide a proper `StatManager` configuration in the Inspector for its base stats.
6. For the sprite color (modulation based on level), it is handled entirely in code! You do NOT need to duplicate scenes or nodes for each color. Simply drop an `Archer.tscn` (or `Soldier.tscn`) into your game and change the `LevelIndex` property on the root node. The `EnemyBase` script will automatically apply the correct color (Yellow, Red, Purple, Black) directly to the `AnimatedSprite2D` via its `SelfModulate` property.
7. Make sure `Arrow.tscn` is created and available at `res://Scenes/Projectiles/Arrow.tscn`.
