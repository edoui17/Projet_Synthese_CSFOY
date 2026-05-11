namespace Core.Interfaces.Entities;

public interface IAgressorController
{
    string CurrentState { get; }
    void Update(float p_delta, bool p_hasTarget, bool p_hasLineOfSight);
    void SetDead();
    void ForceNewDirection();
    void ResetDirectionChangeTimer();
    bool CanAttack();
    void StartAttack();
    void EndAttack();
}
