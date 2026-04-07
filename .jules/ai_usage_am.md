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

### 2024-05-24 - [User Story 4.3] | Inventory Manager Setup | Item Properties, Slot Management, Singleton Persistance | Decision Reasoning
*   **Request:** Implement a full inventory system allowing players to collect, view, and consume resources (gold, rocks, trees) while maintaining this list across different scenes.
*   **AI Contribution:**
    *   Created `ResourceItem` and `InventorySlot` classes inside `Src/Core/Domain`.
    *   Refactored `InventoryManager` (`Src/Core/Managers`) to hold a Dictionary of `InventorySlot` rather than raw primitive counts. Added `GetAllSlots()` to expose readonly details to future UI components.
    *   Added string `IconPath` configurations natively mapped into Godot resources (`Gold`, `Rock`, `AutomnTree`, `ConiferTree`) without pulling `Texture2D` instances across into the Core library.
    *   Refactored Godot `InventoryNode` into an Autoload (Singleton) format using `_EnterTree` Instance assignments so that the underlying Core logic survives scene transitions. Added a `ConsumeItem` helper.
    *   Created extensive Unit Tests covering `InventoryManager` adding, incrementing, removing, and iterating over readonly slot lists within `Tests/UnitTests/Core/Managers/InventoryManagerTests.cs`.
*   **Decision Reasoning:** The `Core` tier must remain pure. Therefore, `IconPath` is stored as a Godot Engine asset path (`res://...`) within a string property rather than attempting to pass an explicit `Godot.Texture2D` object directly to `InventoryManager`, violating N-Tier logic. Converting the primitive counts over to an `InventorySlot` object encapsulates future additions like weights, conditions, or equip logic organically. Making `InventoryNode` a Godot Singleton satisfies Scenario 4 (scene persistence). Since the inventory logic is completely separated, a raw C# Unit Test project easily verifies the system's robustness for future Database (Infrastructure) and Blazor (Web) integrations.

### 2025-02-12 - [User Story 4.3] | Inventory Manager Unit Tests | Create xUnit tests | Decision Reasoning
*   **Request:** Create unit tests in the `Tests/UnitTests` folder for the inventory manager.
*   **AI Contribution:**
    *   Created `InventoryManagerTests.cs` using the xUnit framework.
    *   Implemented standard transaction tests (adding, removing, querying counts, getting all slots).
    *   Implemented edge/limit case tests (null items, empty ids, negative amounts, removing non-existent items).
*   **Decision Reasoning:** The user specifically requested xUnit. No mocking was necessary since `InventoryManager` only depends on simple domain models (`ResourceItem`, `InventorySlot`). Testing limits and negative numbers ensures robust behavior of the core logic according to N-Tier requirements.
### 2026-04-02 - [US 4.2] | Implemented resource spending mechanics | AI mapped SignalManager for OnResourceSpent in Core and Godot, updated InventoryNode to deduct resources, and made InteractionScript handle key events to consume resources and increase Player stats. | Chose to map keys 1-5 in InteractionScript temporarily until UI is present. Updated ISignalManager and Godot wrapper to emit resource spend events safely.

### 2026-04-02 - [User Story 4.2] | Dépenser les ressources | Refactored spending logic to IShopManager, created UI panel for purchases, updated signal architecture for stat upgrades | Decision Reasoning
*   **Request:** Implement the base UI buttons to spend resources (Meat, Wood, Rock, Gold) to upgrade stats (Health, Speed, Attack, Luck) and purchase new islands. Ensure the stats script listens to a signal to increase stats instead of direct coupling. Write unit tests to verify.
*   **AI Contribution:**
    *   Created `IShopManager` and `ShopManager` in `Src/Core` to handle resource validation and cost scaling, keeping Godot logic clean.
    *   Wrote xUnit tests in `Tests/UnitTests/Managers/ShopManagerTests.cs` to test all scaling and purchasing permutations.
    *   Added `StatUpgradePurchased` event in `ISignalManager` and implemented it in both `Core` and `Godot` singletons.
    *   Updated `StatManager.cs` to listen to `StatUpgradePurchased` and automatically apply `AddPermanentBonus`.
    *   Refactored `InteractionScript.cs` to dynamically generate a `CanvasLayer` with UI `Button`s for the shop, removing the hardcoded keyboard keys.
*   **Decision Reasoning:** By creating `IShopManager`, we extract the business logic of scaling prices and validating inventory out of the Godot presentation layer. This aligns perfectly with the N-Tier architecture and enables xUnit tests. Creating the dynamic UI panel directly in the C# `_Ready()` method keeps everything self-contained within the base map interaction logic without requiring a new scene tree configuration. Moving the actual stat upgrade logic to `StatManager` listening to a global signal creates a clean decouple between the base map and the player entity.
