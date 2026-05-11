using System.Collections.Generic;

namespace Core.Domain;

public class SyncRequest
{
    public PlayerStats? Stats { get; set; }
    public PlayerConfig? Config { get; set; }
    public IEnumerable<InventoryEntry>? Inventory { get; set; }
}
