# Forge - Architecture & Discovery Log

## Architectural Discoveries
*   **Live State vs Configuration (Resources)**: Godot Resources (`SessionResource.cs`) act purely as initial templates (Source of Truth). The Core (`SessionState.cs`) maintains the mutable live state. Changes to the state only occur via Core methods, which then synchronize back to Godot through WeakEvents. This separation enables testing Core logic independently of the Godot engine.
*   **Dependency Inversion (Save System)**: To keep the Core agnostic of file systems, `ISaveService` is defined in the Core. Implementations like `GodotSaveService` in the IslandSurvivor project leverage platform-specific tools (`FileAccess`, `user://`), injected into Core managers (`ScoreTracker`) during initialization.
*   **Signal Management Bridge**: Godot's Singleton classes (`SignalManager.cs`) seamlessly wrap Core classes (`SignalManagerCore.cs`). Exposing strongly-typed `WeakEvent` properties natively works in Godot `_Ready()` hooks, creating an airtight boundary between the engine's lifecycle and the Business logic (`Core` tier).
*   **Dependency Avoidance (Enums)**: To avoid using enums across project bounds, String mappings for `MaterialType` correctly proxy the state of an `InventoryManager` inside `Core` when a `WeakEvent` is raised.
*   **State Machine Management (Player)**: By defining `PlayerState` in the Core tier, we ensure that both the engine (for animation control) and potential future Core managers (for logic blocking) share a common language for the character's current activity.

## Godot & C# Quirks
*   **C# Name vs Godot Filenames**: Godot strictly checks `uid` and class names relative to `.tscn` references. If a `.cs` class name mismatches the `script = ExtResource("X")` inside a scene, the Godot editor drops the script. We updated `AutomnTree.tscn` and `ConiferTree.tscn` to fix translation mismatches (`ArbreAutomne` -> `AutomnTree`).
*   **Collision Multi-hit Abuses**: A common Godot physics quirk occurs when an overlapping body (`"Tool"`) registers multiple frame intersections within an `Area2D`. Starting and validating against a `Timer` instance natively prevents this.
*   **Physics-Based Interaction Prioritization**: While Godot handles the proximity detection via `Area2D`, the Core's `InteractionService` calculates the distance-based priority. This allows for unit testing the "best target" selection without spawning nodes.

## Inventory Architecture
*   **Pure Core Domains**: The Core tier cannot parse Godot assets (like `Texture2D`). Therefore, items map their visual components using simple absolute paths (`string IconPath = "res://Assets/..."`). The Godot UI/Client will be responsible for converting this text back into images.
*   **Singleton Scene Persistence**: To satisfy global variable conditions across scene reloads, the `InventoryNode` Godot class relies on the Engine's Autoload mechanics. Internally, the class leverages static instantiation assignments to maintain its reference over its encapsulated `.NET` implementation (`InventoryManager`).
