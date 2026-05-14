# Forge Technical Log - IslandSurvivor

## 2026-05-14 - EnemySpawnZone System
- Implemented `EnemySpawnZone` to handle localized enemy spawning with precise control.
- **Key Discovery:** Using `[GlobalClass]` on `Resource` derived classes like `EnemySpawnConfig` allows them to be easily created and assigned in the Godot inspector.
- **Architecture:** Followed N-Tier by defining `IEnemySpawnZone` in `Core`.
- **Scaling:** Integrated `LevelModifier` (0.0-1.0) which maps to `EnemyBase.LevelIndex` (1-11).
- **Collision:** Enemies spawned via `EnemySpawnZone` are automatically assigned to Collision Layer 4 (Combat).
- **Y-Sort:** Enabled `YSortEnabled` on the zone and spawned enemies to ensure correct depth rendering.
