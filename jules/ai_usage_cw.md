# AI Usage Journal

### 2026-03-22 - [User Story 2.2 : Implémenter l'interaction du joueur]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **User Story 2.2: Implémenter l'interaction du joueur.**<br>- Implement detection, visual indicator, interaction triggering, and prioritization logic (closest object).<br>- **Tasks**: Configure Input, Create Movement Data, Physics Movement, Player Collision, Environment Collision, Validation, Documentation. | **1. Core Logic**: Created `IInteractable` (`Src/Core/Interfaces/`) and `InteractionService` (`Src/Core/Managers/`) to handle agnostically the prioritization of the closest object based on coordinates.<br>**2. Player State**: Defined `PlayerState` enum (`Src/Core/Domain/`) to manage Idle, Moving, and Interacting states via a state machine.<br>**3. Movement Data**: Implemented `PlayerMovementData` as a Godot `Resource` to store acceleration and friction, following the project's data-driven approach.<br>**4. Godot Player**: Refactored `Player.cs` and `Player.tscn` to use `CharacterBody2D`, integrate with `StatManager` (for Speed), and implement the state machine. Added a "Press E" `Label` for visual feedback.<br>**5. Interaction Bridge**: Created `InteractableNode.cs` inheriting from `Area2D` and implementing `IInteractable`, allowing Godot nodes to easily participate in the Core's interaction logic.<br>**6. Scene Cleanup**: Replaced the legacy player scene in `BaseMap` with the standardized version to ensure system consistency. | **N-Tier Separation**: By moving the "brain" (sorting by distance) to the Core's `InteractionService`, we maintain a clean boundary between engine physics (detection) and business logic (selection).<br>**Physics-Based Locomotion**: Switching the player to `CharacterBody2D` and using `MoveAndSlide` with acceleration/friction provides a more responsive and professional feel compared to simple position increments.<br>**Extensibility**: The `InteractableNode` bridge makes it trivial to add new interactable types (NPCs, chests, etc.) by simply inheriting and overriding the `Interact()` method, while automatically benefiting from the proximity detection system. |

### 2026-03-22 - [Task 2.2.5 : Implémenter Attaque du Joueur]

| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| **Task 2.2.5: Implémenter Attaque du Joueur.**<br>- Implement attack triggering (Space key), movement blocking, and hit detection. | **1. Interfaces**: Defined `IAttackable` interface in `Src/IslandSurvivor/Interfaces/`.<br>**2. Attackable Node**: Created `AttackableNode` in `Src/IslandSurvivor/Nodes/` as a base for enemies/objects that can be hit.<br>**3. Player Refactoring**: Updated `Player.cs` to handle the `Attacking` state, play `ATTACK` animation, and block movement. Integrated `m_weaponArea` for hit detection.<br>**4. Tscn Update**: Linked `WeaponInteractionArea` in `Player.tscn`.<br>**5. Verification**: Added unit test for `IAttackable` interface. | **Separation of Concerns**: Kept combat separate from the general interaction system. Used the existing `PlayerState.Attacking` and `AnimationPlayer` frames to enable/disable the weapon hitbox, ensuring precise hit registration. |

### 2025-04-19 - [Persistence] N-Tier Database Schema & EF Core

**Request**: Create database tables and relationships for Inventory, Stats, and PlayerConfig meta-progression. Provide SQL DDL and EF Core implementation.
**AI Contribution**:
1. Created SQL DDL script `schema.sql` with tables for `Players`, `ResourceItems`, `Inventory`, `Stats`, and `PlayerConfig`.
2. Refactored domain models (`Player`, `InventoryEntry`, etc.) into `Src/Core/Domain` to satisfy N-Tier architecture.
3. Defined `IRepository<T>` interface in `Src/Core/Interfaces` following "Interfaces First" rule.
4. Implemented `AppDbContext` in `Src/Infrastructure` mapping Core models via Fluent API.
5. Translated and updated persistence documentation to English (`WikiCode/Persistence_System.md`).
**Decision Reasoning**: Adhering to strict N-Tier and "Interfaces First" requirements ensures long-term maintainability and clean separation of concerns. Moving entities to the Core Domain allows all layers (API, Godot, Infrastructure) to share the same business models without cyclic dependencies.

### 2026-04-20 - [US 7.1.1] | Implementation of DAL and SQL Schema | Defined persistence scope as Meta-Progression and instructed on EF Core integration within Infrastructure layer.

**AI Contribution**:
1. Updated SQL schema with `ExtraStats` column for flexibility.
2. Implemented repository interfaces in Core and implementations in Infrastructure.
3. Mapped Infrastructure entities to Core domain models within the repositories.
4. Documented the use of `ExtraStats` for future-proofing stats.
**Decision Reasoning**: Column-per-stat optimizes reads for core meta-progression, while a JSON column provides the necessary flexibility for future attributes without schema overhead.

### 2026-04-21 - [Cleanup] | Removal of Resolution and IsFullScreen from PlayerConfig | Removed unnecessary fields from PlayerConfig throughout the N-Tier architecture.

**AI Contribution**:
1. Removed fields from `Core.Domain.PlayerConfig` and `Infrastructure.Entities.PlayerConfigEntity`.
2. Updated `AppDbContext` and `PlayerRepository` to eliminate dependencies on these fields.
3. Updated SQL schema script `schema.sql` and `Persistence_System.md` documentation.
4. Initialized and added Entity Framework Core migration `InitialCreate` for the Infrastructure project.
**Decision Reasoning**: Simplifies the persistence model by removing fields not required for meta-progression. Initializing EF migrations ensures the development environment can correctly synchronize the database state with the updated models.

### 2026-04-21 - [US 7.1.2] | Creation of initial configuration data script | Created a DML script to populate the database with default configuration and test data.

**AI Contribution**:
1. Created `Src/Infrastructure/database/script/data.sql` with seed data for all persistence tables.
2. Configured default `Speed` (vitesse de base) to 1.0 for test players.
3. Populated `ResourceItems` with game-matching entities (Wood, Rock, Gold, Meat).
4. Updated `WikiCode/Persistence_System.md` to document the new script.
**Decision Reasoning**: Providing a pre-configured data script allows for immediate testing of the meta-progression system and ensures consistent base configuration (like speed) across different environments.