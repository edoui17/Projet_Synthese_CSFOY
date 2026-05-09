using System.Collections.Generic;

namespace Core.Domain;

public class InventoryUpsertRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public IEnumerable<InventoryEntry> Inventory { get; set; } = null!;
}
