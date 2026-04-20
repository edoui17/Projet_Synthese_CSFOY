using System;

namespace Infrastructure.Entities;

public class InventoryEntity
{
    public Guid PlayerId { get; set; }
    public string ResourceItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public virtual PlayerEntity? Player { get; set; }
    public virtual ResourceItemEntity? ResourceItem { get; set; }
}
