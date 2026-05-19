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
    public float BonusHealth { get; set; }
    public float BonusAttack { get; set; }
    public float BonusSpeed { get; set; }
    public float BonusLuck { get; set; }
    public int Level { get; set; }
    public int Score { get; set; }
    public TimeSpan Duration { get; set; }
}
