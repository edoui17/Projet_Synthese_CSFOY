# Forge Technical Log

## Architectural Discoveries
- **Resource Spawning**: Moved from a specific `TreeZone` to a generic `ResourceZone`. This aligns with the N-Tier goal of flexibility and reuse.
- **Water Validation**: Implemented using `TileMapLayer.GetCellSourceId`. If the ID is not -1, it means a tile exists at that position. By referencing the `WaterTileMap`, we can easily block spawning on water tiles.

## Godot/C# Quirks
- **Collision Layers**: Bit 4 corresponds to Layer 5. In code, this is `1 << 4 = 16`.
- **Y-Sort**: Enabled `YSortEnabled` on the `ResourceZone` and its children to ensure correct depth sorting.
- **Instance Validation**: Used `IsInstanceValid(r)` to check if a resource has been destroyed (`QueueFree`) before counting it towards the total.

## Conventions
- Followed PascalCase for methods and camelCase for local variables.
- Fields prefixed with `m_`.
- Method parameters prefixed with `p_`.
