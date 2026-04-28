# Godot Editor Instructions

## Fix NPCs Phasing Through Walls

The `Sheep` and `Soldier` entities are currently passing through walls because they are missing the correct collision mask bit to detect the map environment (which is typically placed on Layer 1).

### For the Sheep

1. Open `res://Scenes/NPC/Passive/Sheep.tscn`
2. Select the root node `Sheep` (CharacterBody2D)
3. In the Inspector, under **Collision**, open the **Mask** property.
4. **Enable Layer 1**. (Currently it only has Layer 4 or 8 depending on the mask configuration, make sure Layer 1 is checked so it collides with walls).

### For the Soldier

1. Open `res://Scenes/NPC/Agressive/Soldier.tscn`
2. Select the root node `Soldier` (CharacterBody2D)
3. In the Inspector, under **Collision**, open the **Mask** property.
4. **Enable Layer 1**. (It currently has Layer 16 and 5, make sure Layer 1 is checked).