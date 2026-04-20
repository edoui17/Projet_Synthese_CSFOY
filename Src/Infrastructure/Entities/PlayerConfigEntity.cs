using System;

namespace Infrastructure.Entities;

public class PlayerConfigEntity
{
    public Guid PlayerId { get; set; }
    public float MasterVolume { get; set; }
    public float MusicVolume { get; set; }
    public float SfxVolume { get; set; }
    public string? Resolution { get; set; }
    public bool IsFullScreen { get; set; }

    public virtual PlayerEntity? Player { get; set; }
}
