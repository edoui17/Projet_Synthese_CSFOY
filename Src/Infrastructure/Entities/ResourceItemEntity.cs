using System.Collections.Generic;

namespace Infrastructure.Entities;

public class ResourceItemEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? IconPath { get; set; }

    public virtual ICollection<InventoryEntity> InventoryEntries { get; set; } = new List<InventoryEntity>();
}
