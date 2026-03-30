# Forge - Architecture & Discovery Log

## Architectural Discoveries
*   **Signal Management Bridge**: Godot's Singleton classes (`SignalManager.cs`) seamlessly wrap Core classes (`SignalManagerCore.cs`). Exposing strongly-typed `WeakEvent` properties natively works in Godot `_Ready()` hooks, creating an airtight boundary between the engine's lifecycle and the Business logic (`Core` tier).
*   **Dependency Avoidance (Enums)**: To avoid using enums across project bounds, String mappings for `MaterialType` correctly proxy the state of an `InventoryManager` inside `Core` when a `WeakEvent` is raised.

## Godot & C# Quirks
*   **C# Name vs Godot Filenames**: Godot strictly checks `uid` and class names relative to `.tscn` references. If a `.cs` class name mismatches the `script = ExtResource("X")` inside a scene, the Godot editor drops the script. We updated `AutomnTree.tscn` and `ConiferTree.tscn` to fix translation mismatches (`ArbreAutomne` -> `AutomnTree`).
*   **Proximity vs Collision Damage**: Since the resources don't explicitly rely on "health" mechanics yet, `_Input` is the best location to intercept `"interact"`. However, to isolate "interaction" from "global attack", it requires an `Area2D` flag `m_isPlayerNear` initialized and wiped by `AreaEntered`/`AreaExited` against the `Player` group.
