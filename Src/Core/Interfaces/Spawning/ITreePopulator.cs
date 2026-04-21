namespace Core.Interfaces.Spawning;

/// <summary>
/// Interface for objects responsible for spawning trees in the game world.
/// </summary>
public interface ITreePopulator
{
    /// <summary>
    /// Triggers the tree spawning logic.
    /// </summary>
    void Populate();
}
