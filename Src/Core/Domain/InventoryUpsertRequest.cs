using System.Collections.Generic;

namespace Core.Domain;

public class InventoryUpsertRequest
{
    public IEnumerable<InventoryEntry> Inventory { get; set; } = null!;
}
