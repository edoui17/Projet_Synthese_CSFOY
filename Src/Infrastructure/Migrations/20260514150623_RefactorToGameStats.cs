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
            // Drop existing Stats table
            migrationBuilder.DropTable(
                name: "Stats");

            // Add new columns to Players
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

            // Create new GameStats table
            migrationBuilder.CreateTable(
                name: "GameStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    LevelReached = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Score = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Health = table.Column<float>(type: "real", nullable: false),
                    Attack = table.Column<float>(type: "real", nullable: false),
                    Speed = table.Column<float>(type: "real", nullable: false),
                    Luck = table.Column<float>(type: "real", nullable: false),
                    BonusHealth = table.Column<float>(type: "real", nullable: false),
                    BonusAttack = table.Column<float>(type: "real", nullable: false),
                    BonusSpeed = table.Column<float>(type: "real", nullable: false),
                    BonusLuck = table.Column<float>(type: "real", nullable: false),
                    ExtraStats = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameStats_PlayerId",
                table: "GameStats",
                column: "PlayerId");

            // Add Trigger TR_GameStats_AfterInsert
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

            migrationBuilder.CreateTable(
                name: "Stats",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Attack = table.Column<float>(type: "real", nullable: false),
                    ExtraStats = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Health = table.Column<float>(type: "real", nullable: false),
                    Luck = table.Column<float>(type: "real", nullable: false),
                    Speed = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_Stats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
