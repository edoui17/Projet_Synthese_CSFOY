using System.Collections.Generic;

namespace Core.Domain;

public class PlayerProfile
{
    public Player Player { get; set; } = null!;
    public ICollection<InventoryEntry> Inventory { get; set; } = new List<InventoryEntry>();
    public ICollection<GameStats> GameStats { get; set; } = new List<GameStats>();
    public PlayerConfig? Config { get; set; }
}
