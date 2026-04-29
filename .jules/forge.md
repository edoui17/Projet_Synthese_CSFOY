### Godot Collision Logic with Overlapping Areas
When writing AoE combat logic in Godot using `Area2D`, utilizing `GetOverlappingAreas()` or `GetOverlappingBodies()` captures everything in a single frame. However, because entities might be composed of a `CharacterBody2D` (body) with child `Area2D` nodes (hitboxes), scanning both lists can lead to applying damage twice if the entity implements `IDamageable` on the parent node and the child area forwards the reference or also gets casted.
**Solution:** Always funnel the gathered `IDamageable` targets into a `HashSet<IDamageable>` before calling `TakeDamage()`. This ensures that even if the engine detects multiple colliders belonging to the same entity, damage is only applied once per attack.### Godot Technical Discoveries
* Godot CharacterBody2D objects like the Sheep and Soldier must have the map collision layer (typically Layer 1) enabled in their `collision_mask`, regardless of `motion_mode`, to prevent phasing through the environment walls.
* To persist UI synchronization when reloading or changing scenes, the new UI instance should fetch the immediate state from global singletons (like `InventoryNode.Instance.Manager`) during its `_Ready()` lifecycle rather than solely relying on event-driven updates.

### Date: 2026-04-29 - Unified Combat & Gathering Updates
* Ensure that the visual hit flash for damageable entities is handled by a generic extension method (`NodeExtensions.PlayHitFlash`) rather than copy-pasting the Modulate and Timer logic across all scripts.
* Aggressive NPCs should not drop resources, they should only grant points to the `ScoreManager` via `ServiceRegistry.Instance.ScoreTracker`.
