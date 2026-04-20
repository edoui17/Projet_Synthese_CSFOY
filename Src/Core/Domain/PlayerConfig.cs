using System;

namespace Core.Domain;

public class PlayerConfig
{
    public Guid PlayerId { get; set; }
    public float MasterVolume { get; set; }
    public float MusicVolume { get; set; }
    public float SfxVolume { get; set; }
    public string? Resolution { get; set; }
    public bool IsFullScreen { get; set; }

    public virtual Player? Player { get; set; }
}
