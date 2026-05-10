using System.Collections.Generic;

namespace Core.Domain;

public class ProfileResponse
{
    public string Username { get; set; } = string.Empty;
    public PlayerStats? Stats { get; set; }
    public PlayerConfig? Config { get; set; }
    public IEnumerable<InventoryEntry> Inventory { get; set; } = new List<InventoryEntry>();
}
