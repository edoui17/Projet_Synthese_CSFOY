# Forge Technical Log

## 2026-04-24 - Line of Sight Implementation
- **Quirk/Discovery:** When implementing `RayCast2D` checks in the `_PhysicsProcess`, it is important to call `ForceRaycastUpdate()` after modifying `TargetPosition` to ensure the collision check is accurate for the current frame before evaluating `.IsColliding()`. This prevents off-by-one frame lag in detection.
