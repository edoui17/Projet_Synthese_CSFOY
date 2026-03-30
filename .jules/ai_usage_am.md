### 2024-05-24 - [User Story 4.1] | Resource Gathering implementation | Added Interaction, Signal and Inventory Manager | Decision Reasoning
*   **Request:** Implement resource gathering (Gold, Rock, AutomnTree, ConiferTree) so the player can pick them up when nearby using "interact", and update the resources in an inventory using the existing signal manager. Follow strict N-Tier architecture.
*   **AI Contribution:**
    *   Designed `IInventoryManager` and `InventoryManager` inside `Src/Core` to be totally decoupled from Godot.
    *   Refactored `ISignalManager` and its implementations (`SignalManagerCore` and the Godot singleton `SignalManager`) to pass string `MaterialType` and `int quantity` when materials are destroyed.
    *   Refactored resource nodes (`Gold`, `Rock`, `AutomnTree`, `ConiferTree`) to rely on Godot collision layers (`AreaEntered`) checking against the `"Tool"` group instead of native inputs.
    *   Added `Timer.IsStopped()` validation combined with `Timer.Start()` to prevent multi-hit frame abuse by tools overlapping the shape multiple times.
    *   Fired the Global `SignalManager.EmitMaterialDestroyed()`.
    *   Renamed misnamed classes and updated `.tscn` scripts respectively.
*   **Decision Reasoning:** The user explicitly stated no enums were to be used and the `Inventory` should be a `Manager`. Furthermore, a colleague code review resulted in reverting manual inputs (`interact`) on resources to use the combat tool-hit style. The approach uses Godot's group checks on `AreaEntered` against `"Tool"`. It ensures smooth scaling alongside the eventual combat system. The Godot layer observers update the core data independently.
