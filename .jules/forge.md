# Forge - Architecture & Discovery Log

## Architectural Discoveries
*   **Live State vs Configuration (Resources)**: Godot Resources (`SessionResource.cs`) act purely as initial templates (Source of Truth). The Core (`SessionState.cs`) maintains the mutable live state. Changes to the state only occur via Core methods, which then synchronize back to Godot through WeakEvents. This separation enables testing Core logic independently of the Godot engine.
*   **Dependency Inversion (Save System)**: To keep the Core agnostic of file systems, `ISaveService` is defined in the Core. Implementations like `GodotSaveService` in the IslandSurvivor project leverage platform-specific tools (`FileAccess`, `user://`), injected into Core managers (`ScoreTracker`) during initialization.
*   **Signal Management Bridge**: Godot's Singleton classes (`SignalManager.cs`) seamlessly wrap Core classes (`SignalManagerCore.cs`). Exposing strongly-typed `WeakEvent` properties natively works in Godot `_Ready()` hooks, creating an airtight boundary between the engine's lifecycle and the Business logic (`Core` tier).
*   **Dependency Avoidance (Enums)**: To avoid using enums across project bounds, String mappings for `MaterialType` correctly proxy the state of an `InventoryManager` inside `Core` when a `WeakEvent` is raised.
*   **State Machine Management (Player)**: By defining `PlayerState` in the game project, we ensure that the engine can manage animation and logic blocking based on the character's current activity.

## Godot & C# Quirks
*   **C# Name vs Godot Filenames**: Godot strictly checks `uid` and class names relative to `.tscn` references. If a `.cs` class name mismatches the `script = ExtResource("X")` inside a scene, the Godot editor drops the script. We updated `AutomnTree.tscn` and `ConiferTree.tscn` to fix translation mismatches (`ArbreAutomne` -> `AutomnTree`).
*   **Collision Multi-hit Abuses**: A common Godot physics quirk occurs when an overlapping body (`"Tool"`) registers multiple frame intersections within an `Area2D`. Starting and validating against a `Timer` instance natively prevents this.
*   **Physics-Based Interaction Prioritization**: While Godot handles the proximity detection via `Area2D`, the game's `InteractionService` calculates the distance-based priority. This allows for unit testing the "best target" selection without spawning nodes.

## Inventory Architecture
*   **Pure Core Domains**: The Core tier cannot parse Godot assets (like `Texture2D`). Therefore, items map their visual components using simple absolute paths (`string IconPath = "res://Assets/..."`). The Godot UI/Client will be responsible for converting this text back into images.
*   **Singleton Scene Persistence**: To satisfy global variable conditions across scene reloads, the `InventoryNode` Godot class relies on the Engine's Autoload mechanics. Internally, the class leverages static instantiation assignments to maintain its reference over its encapsulated `.NET` implementation (`InventoryManager`).
### Technical Quirks / Discoveries
- **Inventory & Signal Management Godot Interop**: Implemented OnResourceSpent using Core WeakEvents mapping to Godot Singletons. Godot Singleton acts as the glue. InteractionScripts can use the Key inputs to simulate UI upgrades temporarily while we map it to Player StatManager directly.
- **Dynamic Godot UI creation**: Created an interactive UI popup in `InteractionScript.cs` using Godot's built-in UI components (`CanvasLayer`, `Panel`, `VBoxContainer`, `Button`). The UI reacts strictly to player body entries into `Area2D` and handles button presses using lambda actions.
- **Stat upgrade isolation via Signals**: Stats upgrades are kept modular by emitting `StatUpgradePurchased` instead of tightly coupling the base Interaction script to the `StatManager`. The Godot `SignalManager` simply proxies the `Core` logic.
