using System;

namespace Core.Domain;

public class InventoryEntry
{
    public Guid PlayerId { get; set; }
    public string ResourceItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public virtual Player? Player { get; set; }
    public virtual ResourceItem? ResourceItem { get; set; }
}
