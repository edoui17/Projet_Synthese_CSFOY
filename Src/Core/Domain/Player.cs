using System;
using System.Collections.Generic;

namespace Core.Domain;

public class Player
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? SessionToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int HighScore { get; set; }

    public virtual ICollection<InventoryEntry> Inventory { get; set; } = new List<InventoryEntry>();
    public virtual ICollection<GameStats> GameStats { get; set; } = new List<GameStats>();
    public virtual PlayerConfig? Config { get; set; }
}
