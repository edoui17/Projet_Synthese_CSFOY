using System;
using System.Collections.Generic;

namespace Core.Domain;

public class Player
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<InventoryEntry> Inventory { get; set; } = new List<InventoryEntry>();
    public virtual PlayerStats? Stats { get; set; }
    public virtual PlayerConfig? Config { get; set; }
}
