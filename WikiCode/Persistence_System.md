# Persistence and Save System (US 7.1)

This document details the persistence architecture for IslandSurvivor meta-progression (permanent upgrades, persistent inventory, and configurations).

## 1. General Architecture

Persistence follows the project's N-Tier model:
- **Core (`Src/Core`)**: Contains pure domain models (`Player`, `ResourceItem`, etc.) and repository interfaces.
- **Infrastructure (`Src/Infrastructure`)**: Contains the database schema (SQL) and object-relational mapping (EF Core).
- **API (`Src/API`)**: (Coming soon) Will provide endpoints to save and load data from the Godot client.
- **Godot Client (`Src/IslandSurvivor`)**: Will communicate with the API to persist player state.

## 2. Database Schema

The schema is designed for SQL Server and uses **GUIDs (uniqueidentifier)** for primary keys to facilitate synchronization between client and server.

### Main Tables
- **`Players`**: Player identity (Id, Username, CreatedAt).
- **`ResourceItems`**: Catalog of available items (ID as a string to match Core, Name, Type, IconPath).
- **`Inventory`**: Junction table between players and items (PlayerId, ResourceItemId, Quantity).
- **`Stats`**: Meta-progression statistics (PlayerId, Health, Attack, Speed, Luck). These values represent permanent bonuses purchased between runs.
- **`PlayerConfig`**: User settings (PlayerId, Audio Volumes, Resolution, Fullscreen).

The complete DDL script is located at: `Src/Infrastructure/database/script/schema.sql`.

## 3. EF Core Implementation (Infrastructure)

Data access is managed by Entity Framework Core 8 in the `Infrastructure` project.

### Domain Models (`Src/Core/Domain/`)
The database is mapped directly to the domain models in the Core project, ensuring the logic layer remains independent of the database technology.

### DbContext (`AppDbContext.cs`)
The context uses the **Fluent API** to finely configure the model:
- **Composite Keys**: The inventory uses a composite primary key `(PlayerId, ResourceItemId)`.
- **One-to-One Relationships**: The `Stats` and `PlayerConfig` tables are uniquely linked to a `PlayerId`.
- **Cascade Deletes**: If a player is deleted, their inventory, statistics, and configuration are also automatically deleted.

## 4. Usage

Currently, the system is ready to be integrated into the ASP.NET Core API. The next steps will include:
1. Creating Repositories in the Infrastructure layer implementing `IRepository<T>`.
2. Creating Controllers in the API project.
3. Implementing an `ISaveService` in Godot that makes HTTP calls to the API.

## 5. Migration and Evolution
To add a new persistent statistic:
1. Add a column to the `Stats` table in the SQL script.
2. Add the corresponding property to `PlayerStats.cs` in the Core project.
3. Update `AppDbContext.cs` if specific configuration is needed.
