namespace Core.Interfaces;

/// <summary>
/// Interface for objects responsible for spawning enemies in the game world.
/// </summary>
public interface IEnemySpawnZone
{
    /// <summary>
    /// Triggers the enemy spawning logic.
    /// </summary>
    void SpawnEnemies();
}
