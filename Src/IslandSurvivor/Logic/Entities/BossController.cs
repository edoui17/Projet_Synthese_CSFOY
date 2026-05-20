namespace IslandSurvivor.Logic.Entities;

using System;
using Godot;

public class BossController : RangedController, IBossController
{
    private BossPhase m_currentPhase;
    private float m_phaseTimer;
    private readonly float PHASE_CHANGE_INTERVAL = 10.0f; // Change phase every 10 seconds (example)

    public BossPhase CurrentPhase => m_currentPhase;

    public BossController(float p_stoppingDistance = 60.0f) : base(p_stoppingDistance)
    {
        m_currentPhase = BossPhase.Melee;
        m_phaseTimer = PHASE_CHANGE_INTERVAL;
    }

    public void UpdateBoss(float p_delta, bool p_hasTarget, bool p_hasLineOfSight, float p_healthRatio)
    {
        // First, do standard agressor logic
        Update(p_delta, p_hasTarget, p_hasLineOfSight);

        if (CurrentState == NpcStates.DEAD) return;

        // Phase Management
        if (p_healthRatio <= 0.3f)
        {
            m_currentPhase = BossPhase.Enraged;
            UpdateStoppingDistance(60.0f); // Enraged is Melee focused
        }
        else
        {
            // Toggle between Melee and Ranged every 10 seconds
            m_phaseTimer -= p_delta;
            if (m_phaseTimer <= 0)
            {
                m_phaseTimer = PHASE_CHANGE_INTERVAL;
                m_currentPhase = m_currentPhase == BossPhase.Melee ? BossPhase.Ranged : BossPhase.Melee;

                // Adjust stopping distance based on phase
                if (m_currentPhase == BossPhase.Ranged)
                {
                    UpdateStoppingDistance(250.0f);
                }
                else
                {
                    UpdateStoppingDistance(60.0f);
                }
            }
        }
    }
}
