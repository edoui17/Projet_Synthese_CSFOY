# Persistence and Save System (US 7.1)

This document details the persistence architecture for IslandSurvivor meta-progression (permanent upgrades, persistent inventory, and configurations).

## 1. General Architecture

Persistence follows the project's N-Tier model:
- **Core (`Src/Core`)**: Contains pure domain models (`Player`, `ResourceItem`, etc.) and repository interfaces (`IPlayerRepository`, `IInventoryRepository`, `IStatsRepository`).
- **Infrastructure (`Src/Infrastructure`)**: Contains the database schema (SQL), EF Core entities, and repository implementations.
- **API (`Src/API`)**: Provides the data access endpoints and registers the `AppDbContext` using the connection string.
- **Godot Client (`Src/IslandSurvivor`)**: Communicates with the API to persist player state.

## 2. Database Schema

The database is named **`DBIslandSurvivor`**. The schema is designed for SQL Server and uses **GUIDs (uniqueidentifier)** for primary keys.

### Main Tables
- **`Players`**: Player identity (Id, Username, CreatedAt).
- **`ResourceItems`**: Catalog of available items (ID as a string to match Core, Name, Type, IconPath).
- **`Inventory`**: Junction table between players and items (PlayerId, ResourceItemId, Quantity).
- **`Stats`**: Meta-progression statistics (PlayerId, Health, Attack, Speed, Luck).
  - **`ExtraStats`**: A flexible `NVARCHAR(MAX)` column storing JSON for future-proofing statistics without schema migrations.
- **`PlayerConfig`**: User settings (PlayerId, Audio Volumes).

The complete DDL script is located at: `Src/Infrastructure/database/script/schema.sql`.

## 3. Data Access Layer (Infrastructure)

Data access is managed by Entity Framework Core 8 in the `Infrastructure` project.

### Separation of Concerns (Mapping)
To maintain a strict N-Tier architecture, the `Infrastructure` layer uses internal **Entities** (e.g., `PlayerEntity`, `StatsEntity`) for database interaction. The **Repositories** implement the interfaces defined in the `Core` layer and are responsible for mapping these entities to the public **Domain Models** (e.g., `Player`, `PlayerStats`) before returning data to the upper layers.

### DbContext (`AppDbContext.cs`)
The context uses the **Fluent API** to finely configure the model:
- **Composite Keys**: The inventory uses a composite primary key `(PlayerId, ResourceItemId)`.
- **One-to-One Relationships**: The `Stats` and `PlayerConfig` tables are uniquely linked to a `PlayerId`.
- **Cascade Deletes**: If a player is deleted, their inventory, statistics, and configuration are also automatically deleted.

## 4. Usage

The persistence layer is registered in the API's `Program.cs`:
```csharp
builder.Services.AddDbContext<AppDbContext>(p_options =>
    p_options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Current status:
1. Repository interfaces are defined in `Core`.
2. Repositories are implemented in `Infrastructure`.
3. `AppDbContext` is configured in `Infrastructure`.
4. API is configured with the connection string for `DBIslandSurvivor`.

## 5. Migration and Evolution
To add a new persistent statistic:
1. Add a column to the `Stats` table in the SQL script (if it's a primary stat) OR add it to the `ExtraStats` JSON object.
2. Update the corresponding `PlayerStats` domain model in the `Core` project.
3. Update the `StatsEntity` and repository mapping in the `Infrastructure` project.
