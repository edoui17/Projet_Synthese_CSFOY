using System.Collections.Generic;

namespace Core.Domain;

public class SyncRequest
{
    public GameStats? Stats { get; set; }
    public PlayerConfig? Config { get; set; }
    public IEnumerable<InventoryEntry>? Inventory { get; set; }
}
