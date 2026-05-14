using System;

namespace Core.Domain;

public class GameStats
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan Duration { get; set; }
    public int LevelReached { get; set; } = 1;
    public int Score { get; set; }
    public float Health { get; set; }
    public float Attack { get; set; }
    public float Speed { get; set; }
    public float Luck { get; set; }
    public float BonusHealth { get; set; }
    public float BonusAttack { get; set; }
    public float BonusSpeed { get; set; }
    public float BonusLuck { get; set; }
    public string? ExtraStats { get; set; }

    public virtual Player? Player { get; set; }
}
