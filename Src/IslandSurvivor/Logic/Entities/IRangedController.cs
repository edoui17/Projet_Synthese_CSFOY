namespace IslandSurvivor.Logic.Entities;

using Godot;

public interface IRangedController : IAgressorController
{
    float StoppingDistance { get; }
    void UpdateStoppingDistance(float p_distance);
}
