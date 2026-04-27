# Godot Setup Instructions (Combat System)

## Player Node Setup
1. **Area2D Configuration (`m_weaponArea`)**:
   - Verify that the `Player` scene has an `Area2D` child node configured as the attack hitbox (assigned to `m_weaponArea` in the script export).
   - **Collision Layer**: Should be set to an appropriate layer for the player's weapon (e.g., Layer 4).
   - **Collision Mask**: Must include the layers for Enemies (e.g., Layer 3) and Resources (e.g., Layer 5) so `GetOverlappingAreas()` and `GetOverlappingBodies()` can detect them.

## Resource Nodes (Rock, Gold, Trees)
1. **Collision Mask/Layer**:
   - Ensure the `Area2D` nodes for `Rock`, `Gold`, `AutomnTree`, and `ConiferTree` are on the collision layer that the Player's weapon Area2D is scanning (e.g., Layer 5).
   - Ensure the scripts have `Stats` exported and linked to their `StatManager` child node.

## Enemy Nodes (Soldier)
1. **Collision Mask/Layer**:
   - Ensure the `CharacterBody2D` (or its child hitbox Area2D) is on the collision layer scanned by the Player's weapon (e.g., Layer 3).