using System.Collections.Generic;

namespace Core.Domain;

public class ProfileResponse
{
    public string Username { get; set; } = string.Empty;
    public int HighScore { get; set; }
    public IEnumerable<GameStats> LastSessions { get; set; } = new List<GameStats>();
    public PlayerConfig? Config { get; set; }
    public IEnumerable<InventoryEntry> Inventory { get; set; } = new List<InventoryEntry>();
}
