using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToGameStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Rename existing Stats table to GameStats (instead of dropping it)
            migrationBuilder.RenameTable(
                name: "Stats",
                newName: "GameStats");

            // 2. Add new columns to GameStats
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "GameStats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTime>(
                name: "PlayedAt",
                table: "GameStats",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "GameStats",
                type: "time",
                nullable: false,
                defaultValue: TimeSpan.Zero);

            migrationBuilder.AddColumn<int>(
                name: "LevelReached",
                table: "GameStats",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "GameStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "BonusHealth",
                table: "GameStats",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "BonusAttack",
                table: "GameStats",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "BonusSpeed",
                table: "GameStats",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "BonusLuck",
                table: "GameStats",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            // 3. Change Primary Key of GameStats (from PlayerId to Id)
            migrationBuilder.DropPrimaryKey(
                name: "PK_Stats",
                table: "GameStats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameStats",
                table: "GameStats",
                column: "Id");

            // 4. Add columns to Players
            migrationBuilder.AddColumn<int>(
                name: "HighScore",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Players",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            // 5. Add Index on PlayerId for GameStats
            migrationBuilder.CreateIndex(
                name: "IX_GameStats_PlayerId",
                table: "GameStats",
                column: "PlayerId");

            // 6. Add Trigger TR_GameStats_AfterInsert
            migrationBuilder.Sql(@"
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
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_GameStats_AfterInsert");

            migrationBuilder.DropTable(
                name: "GameStats");

            migrationBuilder.DropColumn(
                name: "HighScore",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Players");
        }
    }
}
