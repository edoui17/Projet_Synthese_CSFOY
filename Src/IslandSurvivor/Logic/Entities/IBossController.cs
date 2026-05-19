namespace IslandSurvivor.Logic.Entities;

public enum BossPhase
{
    Melee,
    Ranged,
    Enraged
}

public interface IBossController : IAgressorController
{
    BossPhase CurrentPhase { get; }
    void UpdateBoss(float p_delta, bool p_hasTarget, bool p_hasLineOfSight, float p_healthRatio);
}
