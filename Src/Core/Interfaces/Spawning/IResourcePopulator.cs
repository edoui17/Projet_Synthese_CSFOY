namespace Core.Interfaces;

/// <summary>
/// Interface for objects responsible for spawning resources in the game world.
/// </summary>
public interface IResourcePopulator
{
    /// <summary>
    /// Triggers the resource spawning logic.
    /// </summary>
    void Populate();
}
