namespace IslandSurvivor.Logic.Entities;

using Godot;

public interface ILancerController : IAgressorController
{
    float MinDashDistance { get; set; }
    float MeleeDistance { get; }
    void UpdateDistanceToTarget(float p_distance);
    void UpdateTargetPositions(Vector2 p_agressorPos, Vector2 p_targetPos);
    void StartMelee();
    void FinishMelee();
    void StartDashWindUp();
    void StartDash();
    void FinishDash();
    void FinishRecovery();
}
