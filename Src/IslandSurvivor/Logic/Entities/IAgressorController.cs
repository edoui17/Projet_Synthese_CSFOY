namespace IslandSurvivor.Logic.Entities;

using Godot;

public interface IAgressorController
{
    string CurrentState { get; }
    Vector2 CurrentDirection { get; }
    void Update(float p_delta, bool p_hasTarget, bool p_hasLineOfSight);
    void UpdateChaseDirection(Vector2 p_agressorPosition, Vector2 p_targetPosition);
    void SetDead();
    void ForceNewDirection();
    void ResetDirectionChangeTimer();
}
