using System.Linq;
using Core.Domain;

namespace Core.Utils;

public static class ProfileMapper
{
    public static PlayerProfile MapToDomain(ProfileResponse p_response)
    {
        return new PlayerProfile
        {
            Player = new Player
            {
                Username = p_response.Username ?? "Unknown",
                HighScore = p_response.HighScore,
                UpdatedAt = p_response.UpdatedAt,
                Config = p_response.Config
            },
            Inventory = p_response.Inventory?.ToList() ?? new List<InventoryEntry>(),
            GameStats = p_response.LastSessions?.ToList() ?? new List<GameStats>(),
            Config = p_response.Config
        };
    }
}
