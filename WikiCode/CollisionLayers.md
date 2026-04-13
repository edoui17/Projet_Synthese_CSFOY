# Collision Layers & Masks in Island Survivor

To properly organize physics interactions between different entities (Player, Resources, Interactables, Environment), the project uses specialized collision layers.

## Godot Physics Layers defined

1. **Environnement (Layer 1):**
   - Used for static environment structures (ground, walls, boundaries, TileMap limits) that block movement.
2. **Interaction (Layer 2):**
   - Used for objects the player can interact with (e.g., `BuildingNode`, `Portal`).
3. **Player (Layer 3):**
   - Used by the main `CharacterBody2D` of the player.
4. **Combat (Layer 4):**
   - Used by player weapons and tools during attack animations (`WeaponInteractionArea`).
5. **Ressource (Layer 5):**
   - Used by resource entities (`Gold`, `Rock`, `Trees`, `Sheep`) that can be attacked/farmed.

## Common Setups

* **Player:**
  * Root node (`CharacterBody2D`): `collision_layer = 4` (Player), `collision_mask = 1` (Environnement).
  * `PlayerInteraction` (`Area2D`): `collision_layer = 0`, `collision_mask = 2` (Interaction). This area detects objects.
  * `WeaponInteractionArea` (`Area2D`): `collision_layer = 8` (Combat), `collision_mask = 16` (Ressource). The weapon hits resources.
* **BuildingNode / Portals (Interactables):**
  * `InteractionArea` (`Area2D`): `collision_layer = 2` (Interaction), `collision_mask = 4` (Player). Let the player detect them.
  * *Note*: If the node implements `IInteractable` and listens to `BodyExited` or similar signals from the player leaving, ensure `monitoring = true` is active.
* **Resources (Gold, Rock, Trees, Sheep):**
  * Root node (`Area2D` or `CharacterBody2D`): `collision_layer = 16` (Ressource), `collision_mask = 8` (Combat). Detects weapon hits to take damage.
