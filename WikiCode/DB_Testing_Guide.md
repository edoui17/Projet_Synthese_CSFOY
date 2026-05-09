# Database Testing Guide (MSSQL)

**Date**: May 9, 2026

## 1. Database Initialization
To reset and initialize the database environment for **IslandSurvivor**, follow these steps using SQL Server Management Studio (SSMS) or the `sqlcmd` utility.

### A. Reset Schema
Run the following script to drop and recreate the tables. Ensure you are targeting the `DBIslandSurvivor` database.
- **File**: `Src/Infrastructure/database/script/schema.sql`

### B. Seed Test Data
Run the following script to populate the database with test players, resource items, and initial stats.
- **File**: `Src/Infrastructure/database/script/data.sql`

---

## 2. Validation Queries
Use these three key SELECT queries to verify that the data is correctly linked across tables.

### Query 1: Verify Player Stats Link
This query checks if the stats are correctly associated with each player.
```sql
SELECT p.Username, s.Health, s.Attack, s.Speed, s.Luck
FROM Players p
JOIN Stats s ON p.Id = s.PlayerId;
```

### Query 2: Verify Player Inventory
This query lists all items currently held by players, including the item names from the `ResourceItems` table.
```sql
SELECT p.Username, r.Name AS ItemName, i.Quantity
FROM Players p
JOIN Inventory i ON p.Id = i.PlayerId
JOIN ResourceItems r ON i.ResourceItemId = r.Id;
```

### Query 3: Verify Player Configuration
This query confirms that audio settings are correctly persisted for each player.
```sql
SELECT p.Username, c.MasterVolume, c.MusicVolume, c.SfxVolume
FROM Players p
JOIN PlayerConfig c ON p.Id = c.PlayerId;
```

---

## 3. Manual Reset Procedure
1. Open SSMS.
2. Connect to your LocalDB or SQL Server instance.
3. Open a new query window.
4. Copy-paste the content of `schema.sql` and execute (F5).
5. Copy-paste the content of `data.sql` and execute (F5).
6. Run the validation queries above to confirm success.
