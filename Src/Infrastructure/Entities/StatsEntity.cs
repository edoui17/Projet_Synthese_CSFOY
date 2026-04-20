using System;

namespace Infrastructure.Entities;

public class StatsEntity
{
    public Guid PlayerId { get; set; }
    public float Health { get; set; }
    public float Attack { get; set; }
    public float Speed { get; set; }
    public float Luck { get; set; }
    public string? ExtraStats { get; set; }

    public virtual PlayerEntity? Player { get; set; }
}
