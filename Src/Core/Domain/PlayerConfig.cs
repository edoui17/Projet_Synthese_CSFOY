using System;

namespace Core.Domain;

public class PlayerConfig
{
    public Guid PlayerId { get; set; }
    public float MasterVolume { get; set; }
    public float MusicVolume { get; set; }
    public float SfxVolume { get; set; }

    public virtual Player? Player { get; set; }
}
