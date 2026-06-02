using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain;

public class InventoryEntry
{
    public Guid PlayerId { get; set; }

    [StringLength(100)]
    public string ResourceItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public virtual Player? Player { get; set; }
    public virtual ResourceItem? ResourceItem { get; set; }
}
