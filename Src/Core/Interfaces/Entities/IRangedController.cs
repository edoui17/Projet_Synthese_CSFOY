namespace Core.Interfaces.Entities;

using System.Numerics;

public interface IRangedController : IAgressorController
{
    float StoppingDistance { get; }
    void UpdateStoppingDistance(float p_distance);
}
