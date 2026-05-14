[Output truncated for brevity]

## 2026-05-14 - Meta-Progression & HighScore Refactoring (US 18.0)
- **Database Evolution**: Migrated the `Stats` table from a 1-to-1 relationship with `Players` to a 1-to-many relationship under the new name `GameStats`.
- **Relationship Quirk**: Moving from 1-to-1 to 1-to-many required updating the `PlayerEntity` navigation property to `ICollection<GameStatsEntity>`. This allows tracking full session history.
- **SQL Trigger Logic**: Implemented `TR_GameStats_AfterInsert` directly in the EF Core migration. The trigger automatically updates the `HighScore` in the `Players` table and prunes the `GameStats` history to keep only the top 10 sessions per player.
- **Mapping Duration**: Mapped the SQL `TIME` type to C# `TimeSpan`. Note: EF Core handles this natively, but Ensure the column is defined as `TimeSpan` in the Entity for proper mapping to the `TIME` SQL type.
- **Data Preservation**: Manually edited the EF Core migration to use `RenameTable` instead of `DropTable` to ensure that existing player stats are not lost during the transition to the new `GameStats` schema.
- **API Strategy**: Updated `Sync` endpoint to append new sessions to `GameStats` rather than overwriting a single record. The `Profile` endpoint now returns the `HighScore` and the list of best sessions.
