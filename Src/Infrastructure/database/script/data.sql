-- Seed Data for DBIslandSurvivor
-- This script populates the database with initial configuration and test data.

-- Clear existing data to ensure a clean state
DELETE FROM Inventory;
DELETE FROM Stats;
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
INSERT INTO Players (Id, Username) VALUES
(@ForgeId, 'Forge'),
(@JulesId, 'Jules'),
(@TestPlayerId, 'TestPlayer');

-- 4. Populate Stats
-- Initial values: Speed (vitesse de base) is set to 1 as per requirements.
-- Although columns are FLOAT, values are provided as integers where requested.
INSERT INTO Stats (PlayerId, Health, Attack, Speed, Luck, ExtraStats) VALUES
(@ForgeId, 100, 10, 1, 1, '{"Seeded": true, "Role": "Lead Architect"}'),
(@JulesId, 150, 15, 1, 2, '{"Seeded": true, "Role": "AI Agent"}'),
(@TestPlayerId, 80, 5, 1, 1, '{"Seeded": true}');

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
