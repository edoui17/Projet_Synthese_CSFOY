namespace Core.Domain.Models;

public record IslandDestination(string Id, string ScenePath, string Biome, int Difficulty, int ResourceCost, int DangerLevel)
{
    public static readonly IslandDestination HomeIsland = new(
        Id: "home_hub",
        ScenePath: "res://Scenes/Level/PlayerHub/PlayerHub.tscn",
        Biome: "Meadow",
        Difficulty: 0,
        ResourceCost: 0,
        DangerLevel: 0
    );
}
