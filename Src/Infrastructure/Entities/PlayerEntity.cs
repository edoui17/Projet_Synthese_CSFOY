using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public class PlayerEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<InventoryEntity> Inventory { get; set; } = new List<InventoryEntity>();
    public virtual StatsEntity? Stats { get; set; }
    public virtual PlayerConfigEntity? Config { get; set; }
}
