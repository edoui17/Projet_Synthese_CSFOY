using System.Collections.Generic;

namespace Core.Domain;

public class PlayerProfile
{
    public Player Player { get; set; } = null!;
    public IEnumerable<InventoryEntry> Inventory { get; set; } = new List<InventoryEntry>();
    public IEnumerable<GameStats> GameStats { get; set; } = new List<GameStats>();
    public PlayerConfig? Config { get; set; }
}
