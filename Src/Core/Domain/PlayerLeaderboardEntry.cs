using System;

namespace Core.Domain;

public class PlayerLeaderboardEntry
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public float Health { get; set; }
    public float Attack { get; set; }
    public float Speed { get; set; }
    public float Luck { get; set; }
    public int Level { get; set; }
}
