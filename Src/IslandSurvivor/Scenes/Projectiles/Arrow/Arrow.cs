namespace IslandSurvivor.Scenes.Projectiles;

using Godot;
using IslandSurvivor.Logic;

public partial class Arrow : BaseProjectile
{
    // The BaseProjectile handles Initialize, Fire, _PhysicsProcess, and collision.
    // We only need to override specific behavior if Arrow differs from the base.
}
