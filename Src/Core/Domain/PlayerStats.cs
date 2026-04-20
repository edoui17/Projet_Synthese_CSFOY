using System;

namespace Core.Domain;

public class PlayerStats
{
    public Guid PlayerId { get; set; }
    public float Health { get; set; }
    public float Attack { get; set; }
    public float Speed { get; set; }
    public float Luck { get; set; }

    public virtual Player? Player { get; set; }
}
