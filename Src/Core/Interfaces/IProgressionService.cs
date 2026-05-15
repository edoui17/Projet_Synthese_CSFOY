namespace Core.Interfaces;

using Core.Domain;

public interface IProgressionService
{
    /// <summary>
    /// Calculates the player level based on the session score.
    /// </summary>
    /// <param name="p_score">The score achieved in the session.</param>
    /// <returns>The calculated level.</returns>
    int CalculateLevel(int p_score);

    /// <summary>
    /// Checks if the session score is higher than the player's current high score
    /// and updates the player's in-memory high score if it is.
    /// </summary>
    /// <param name="p_player">The player object.</param>
    /// <param name="p_sessionScore">The score achieved in the session.</param>
    /// <returns>True if the high score was updated, false otherwise.</returns>
    bool CheckAndUpdateHighScore(Player p_player, int p_sessionScore);
}
