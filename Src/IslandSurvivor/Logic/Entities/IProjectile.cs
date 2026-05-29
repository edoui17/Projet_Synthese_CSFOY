namespace IslandSurvivor.Logic;

using Godot;

public interface IProjectile
{
    float Speed { get; }
    float Damage { get; }
    Vector2 Direction { get; }

    void Initialize(Vector2 p_startPosition, Vector2 p_direction, float p_damage, object p_shooter);
    void Fire();
}
