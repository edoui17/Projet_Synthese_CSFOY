namespace IslandSurvivor.Logic.Entities;

using Godot;

public interface ILancerController : IAgressorController
{
    float MinDashDistance { get; }
    float MeleeDistance { get; }
    void UpdateDistanceToTarget(float p_distance);
    void StartMelee();
    void FinishMelee();
    void StartDashWindUp();
    void StartDash();
    void FinishDash();
    void FinishRecovery();
}
