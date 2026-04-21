-- Create Tables for IslandSurvivor Meta-Progression
-- Target Database: DBIslandSurvivor

CREATE TABLE Players (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
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

CREATE TABLE Stats (
    PlayerId UNIQUEIDENTIFIER PRIMARY KEY,
    Health FLOAT NOT NULL DEFAULT 0,
    Attack FLOAT NOT NULL DEFAULT 0,
    Speed FLOAT NOT NULL DEFAULT 0,
    Luck FLOAT NOT NULL DEFAULT 0,
    ExtraStats NVARCHAR(MAX) NULL, -- JSON column for future flexibility
    CONSTRAINT FK_Stats_Players FOREIGN KEY (PlayerId) REFERENCES Players(Id) ON DELETE CASCADE
);

CREATE TABLE PlayerConfig (
    PlayerId UNIQUEIDENTIFIER PRIMARY KEY,
    MasterVolume FLOAT NOT NULL DEFAULT 1.0,
    MusicVolume FLOAT NOT NULL DEFAULT 1.0,
    SfxVolume FLOAT NOT NULL DEFAULT 1.0,
    CONSTRAINT FK_PlayerConfig_Players FOREIGN KEY (PlayerId) REFERENCES Players(Id) ON DELETE CASCADE
);
