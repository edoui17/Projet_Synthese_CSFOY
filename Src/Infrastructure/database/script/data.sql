-- Seed Data for DBIslandSurvivor
-- This script populates the database with initial configuration and test data.
-- Clear existing data to ensure a clean state
USE DBIslandSurvivor;

DELETE FROM Inventory;
DELETE FROM GameStats;
DELETE FROM PlayerConfig;
DELETE FROM Players;
DELETE FROM ResourceItems;

-- 1. Populate ResourceItems
-- These match the IDs used in the Godot client and Core domain logic.
INSERT INTO ResourceItems (Id, Name, Type, IconPath) VALUES
('wood_01', 'Wood', 'Material', 'res://Assets/Images/wood.png'),
('rock_01', 'Rock', 'Material', 'res://Assets/Images/rock.png'),
('gold_01', 'Gold', 'Material', 'res://Assets/Images/gold.png'),
('meat', 'Sheep Meat', 'Food', 'res://Assets/Images/meat.png');

-- 2. Define Player IDs for referencing
DECLARE @ForgeId UNIQUEIDENTIFIER = NEWID();
DECLARE @JulesId UNIQUEIDENTIFIER = NEWID();
DECLARE @TestPlayerId UNIQUEIDENTIFIER = NEWID();

-- 3. Populate Players
INSERT INTO Players (Id, Username, PasswordHash, HighScore) VALUES
(@ForgeId, 'Forge', 'password123', 500),
(@JulesId, 'Jules', 'agent_secret', 750),
(@TestPlayerId, 'TestPlayer', 'test', 0);

-- 4. Populate GameStats (Session History)
-- Using TIME format for Duration 'HH:MM:SS'
INSERT INTO GameStats (Id, PlayerId, PlayedAt, Duration, LevelReached, Score, Health, Attack, Speed, Luck, ExtraStats) VALUES
(NEWID(), @ForgeId, DATEADD(hour, -2, GETDATE()), '00:15:30', 5, 500, 100, 10, 1, 1, '{"Seeded": true}'),
(NEWID(), @JulesId, DATEADD(hour, -1, GETDATE()), '00:20:45', 7, 750, 150, 15, 1, 2, '{"Seeded": true}'),
(NEWID(), @TestPlayerId, GETDATE(), '00:05:00', 2, 150, 80, 5, 1, 1, '{"Seeded": true}');

-- 5. Populate PlayerConfig (Audio settings)
INSERT INTO PlayerConfig (PlayerId, MasterVolume, MusicVolume, SfxVolume) VALUES
(@ForgeId, 0.8, 0.5, 1.0),
(@JulesId, 1.0, 1.0, 1.0),
(@TestPlayerId, 0.5, 0.2, 0.5);

-- 6. Populate Inventory (Initial items for test players)
INSERT INTO Inventory (PlayerId, ResourceItemId, Quantity) VALUES
(@ForgeId, 'wood_01', 10),
(@ForgeId, 'rock_01', 5),
(@JulesId, 'gold_01', 100),
(@TestPlayerId, 'meat', 1);
