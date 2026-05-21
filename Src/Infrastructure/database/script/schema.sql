-- Create Tables for IslandSurvivor Meta-Progression
-- Target Database: DBIslandSurvivor

USE master;
GO
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'DBIslandSurvivor')
    DROP DATABASE DBIslandSurvivor;
GO
CREATE DATABASE DBIslandSurvivor;
GO
USE DBIslandSurvivor;
GO

-- Cleanup existing tables in reverse order of dependencies
IF OBJECT_ID('dbo.PlayerConfig', 'U') IS NOT NULL DROP TABLE dbo.PlayerConfig;
IF OBJECT_ID('dbo.GameStats', 'U') IS NOT NULL DROP TABLE dbo.GameStats;
IF OBJECT_ID('dbo.Inventory', 'U') IS NOT NULL DROP TABLE dbo.Inventory;
IF OBJECT_ID('dbo.ResourceItems', 'U') IS NOT NULL DROP TABLE dbo.ResourceItems;
IF OBJECT_ID('dbo.Players', 'U') IS NOT NULL DROP TABLE dbo.Players;

CREATE TABLE Players (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL DEFAULT '',
    SessionToken NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    HighScore INT NOT NULL DEFAULT 0
);

CREATE TABLE ResourceItems (
    Id NVARCHAR(100) PRIMARY KEY, -- Using string ID to match Core.Domain.ResourceItem.Id
    Name NVARCHAR(100) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    IconPath NVARCHAR(255) NULL
);

CREATE TABLE Inventory (
    PlayerId UNIQUEIDENTIFIER NOT NULL,
    ResourceItemId NVARCHAR(100) NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Inventory PRIMARY KEY (PlayerId, ResourceItemId),
    CONSTRAINT FK_Inventory_Players FOREIGN KEY (PlayerId) REFERENCES Players(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Inventory_ResourceItems FOREIGN KEY (ResourceItemId) REFERENCES ResourceItems(Id) ON DELETE CASCADE
);

CREATE TABLE GameStats (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PlayerId UNIQUEIDENTIFIER NOT NULL,
    PlayedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    Duration TIME NOT NULL,
    LevelReached INT NOT NULL DEFAULT 1,
    Score INT NOT NULL DEFAULT 0,
    Health REAL NOT NULL DEFAULT 0,
    Attack REAL NOT NULL DEFAULT 0,
    Speed REAL NOT NULL DEFAULT 0,
    Luck REAL NOT NULL DEFAULT 0,
    BonusHealth REAL NOT NULL DEFAULT 0,
    BonusAttack REAL NOT NULL DEFAULT 0,
    BonusSpeed REAL NOT NULL DEFAULT 0,
    BonusLuck REAL NOT NULL DEFAULT 0,
    ExtraStats NVARCHAR(MAX) NULL, -- JSON column for future flexibility
    CONSTRAINT FK_GameStats_Players FOREIGN KEY (PlayerId) REFERENCES Players(Id) ON DELETE CASCADE
);
GO

CREATE TRIGGER TR_GameStats_AfterInsert
ON GameStats
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Update HighScore in Players
    UPDATE p
    SET p.HighScore = x.MaxScore,
        p.UpdatedAt = GETDATE()
    FROM Players p
    INNER JOIN (
        SELECT PlayerId, MAX(Score) as MaxScore
        FROM inserted
        GROUP BY PlayerId
    ) x ON p.Id = x.PlayerId
    WHERE x.MaxScore > p.HighScore;

    -- Delete rows outside of top 10 best scores for each affected player
    DELETE FROM GameStats
    WHERE Id IN (
        SELECT Id
        FROM (
            SELECT Id,
                   ROW_NUMBER() OVER(PARTITION BY PlayerId ORDER BY Score DESC, PlayedAt DESC) as rn
            FROM GameStats
            WHERE PlayerId IN (SELECT PlayerId FROM inserted)
        ) x
        WHERE x.rn > 10
    );
END;
GO

CREATE TABLE PlayerConfig (
    PlayerId UNIQUEIDENTIFIER PRIMARY KEY,
    MasterVolume REAL NOT NULL DEFAULT 1.0,
    MusicVolume REAL NOT NULL DEFAULT 1.0,
    SfxVolume REAL NOT NULL DEFAULT 1.0,
    CONSTRAINT FK_PlayerConfig_Players FOREIGN KEY (PlayerId) REFERENCES Players(Id) ON DELETE CASCADE
);
