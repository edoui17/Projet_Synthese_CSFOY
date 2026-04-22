### 2025-05-14 - [Persistence] N-Tier Database Schema & EF Core
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
