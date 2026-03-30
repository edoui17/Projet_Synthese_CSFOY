### 2024-05-24 - [User Story 4.1] | Resource Gathering implementation | Added Interaction, Signal and Inventory Manager | Decision Reasoning
*   **Request:** Implement resource gathering (Gold, Rock, AutomnTree, ConiferTree) so the player can pick them up when nearby using "interact", and update the resources in an inventory using the existing signal manager. Follow strict N-Tier architecture.
*   **AI Contribution:**
    *   Designed `IInventoryManager` and `InventoryManager` inside `Src/Core` to be totally decoupled from Godot.
    *   Refactored `ISignalManager` and its implementations (`SignalManagerCore` and the Godot singleton `SignalManager`) to pass string `MaterialType` and `int quantity` when materials are destroyed.
    *   Added `m_isPlayerNear` flag in each resource node (`Gold`, `Rock`, `AutomnTree`, `ConiferTree`) tracked via `AreaEntered`/`AreaExited` for the "Player" group.
    *   Implemented `_Input` to detect `"interact"`, firing the Global `SignalManager.EmitMaterialDestroyed()`.
    *   Renamed misnamed classes and updated `.tscn` scripts respectively.
*   **Decision Reasoning:** The user explicitly stated no enums were to be used and the `Inventory` should be a `Manager`. The approach safely maintains boundaries by using dependency injection principles for signals. The Godot layer observes events to bind Input Map checks ("interact") directly to the global C# standard library signals within Core.
