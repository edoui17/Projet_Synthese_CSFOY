# Procedural Map Generation

## Overview
The Procedural Map Generation system in Island Survivor follows a strict N-Tier architecture, separating the core generation logic from the Godot rendering engine.

## Core Architecture
Located in `Src/Core/`:
- **`IMapData` & `MapData`**: Defines the data structure representing the map. It holds the dimensions, the seed, and the tile types in a 2D string array.
- **`IMapGenerator`**: Interface defining the contract for generating maps and applying island-specific constraints.
- **`ISpawnLocator` & `SpawnLocator`**: Defines and implements the algorithm to find a valid and safe spawn point for the player based strictly on the `IMapData`.
- **`MapManager`**: The orchestration unit in the Core project. It interacts with the `IMapGenerator` and `ISpawnLocator` to return a fully constructed map and player spawn.
- **`TileTypeConstants`**: Strings defining standard tile types, ensuring no magic strings are used across logic.

## Godot Implementation
Located in `Src/IslandSurvivor/`:
- **`GodotIslandGenerator`**: Implements `IMapGenerator` using `Godot.FastNoiseLite`. This bridges the Godot noise algorithms with our Core logic.
  - **Generation**: Evaluates 2D Simplex noise to determine base "Water" and "Ground" tiles.
  - **Constraints**:
    1. Applies a radial gradient mask to guarantee the edges of the map are water.
    2. Uses a Flood Fill (Breadth-First Search) algorithm from the center to ensure the map consists of a single, connected landmass. Unconnected landmasses are converted to water.
- **Rendering**: The `ProceduralMapTest` scene demonstrates how the string values from `IMapData` are mapped onto the `TileMapLayer` by setting specific Atlas coordinates.
