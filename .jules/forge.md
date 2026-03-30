# Forge - Architecture & Discovery Log

## Architectural Discoveries
*   **Signal Management Bridge**: Godot's Singleton classes (`SignalManager.cs`) seamlessly wrap Core classes (`SignalManagerCore.cs`). Exposing strongly-typed `WeakEvent` properties natively works in Godot `_Ready()` hooks, creating an airtight boundary between the engine's lifecycle and the Business logic (`Core` tier).
*   **Dependency Avoidance (Enums)**: To avoid using enums across project bounds, String mappings for `MaterialType` correctly proxy the state of an `InventoryManager` inside `Core` when a `WeakEvent` is raised.

## Godot & C# Quirks
*   **C# Name vs Godot Filenames**: Godot strictly checks `uid` and class names relative to `.tscn` references. If a `.cs` class name mismatches the `script = ExtResource("X")` inside a scene, the Godot editor drops the script. We updated `AutomnTree.tscn` and `ConiferTree.tscn` to fix translation mismatches (`ArbreAutomne` -> `AutomnTree`).
*   **Collision Multi-hit Abuses**: A common Godot physics quirk occurs when an overlapping body (`"Tool"`) registers multiple frame intersections within an `Area2D`. Starting and validating against a `Timer` instance natively prevents this.
