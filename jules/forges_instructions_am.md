# Godot Editor Setup Instructions

## 1. Soldier Node Collision Fix (Bug 2.3.8)

Currently, the player is unable to attack the `Soldier` because its Collision Layer is incorrect.

To fix this:
1. Open `Src/IslandSurvivor/Scenes/NPC/Agressive/Soldier.tscn` in the Godot Editor.
2. Select the Root node (`Soldier`).
3. In the Inspector, go to the **Collision** section under **CollisionObject2D**.
4. Change the **Layer** mask. It is currently on Layer 4 (Value 8, the Weapon layer). Uncheck Layer 4 and check **Layer 5 (Value 16)**, which is the "Enemies/Resources" layer.
5. Save the scene.

With this change, the Player's weapon (which relies on `collision_mask = 16`) will properly intersect and trigger the attack logic against the Soldier.