namespace IslandSurvivor.Logic.Entities;

using System;
using Godot;

public class LancerController : AgressorController, ILancerController
{
    public float MinDashDistance { get; } = 150f;
    public float MeleeDistance { get; } = 50f;
    public float DashCooldown { get; } = 3.0f; // Seconds between dashes

    private float m_distanceToTarget = float.MaxValue;
    private float m_dashCooldownTimer = 0f;

    public LancerController() : base()
    {
    }

    public void UpdateDistanceToTarget(float p_distance)
    {
        m_distanceToTarget = p_distance;
    }

    public override void Update(float p_delta, bool p_hasTarget, bool p_hasLineOfSight)
    {
        if (m_currentState == NpcStates.DEAD) return;

        if (m_dashCooldownTimer > 0f)
        {
            m_dashCooldownTimer -= p_delta;
        }

        // Lancer specific states (animations handling these largely, we just wait for them to finish)
        if (m_currentState == LancerStates.WIND_UP ||
            m_currentState == LancerStates.DASHING ||
            m_currentState == LancerStates.RECOVERY)
        {
            return; // We don't change state during these hard-committed actions
        }

        if (m_currentState == LancerStates.MELEE)
        {
            // Tactical re-evaluation: if player retreats, finish melee
            if (!p_hasTarget || !p_hasLineOfSight || m_distanceToTarget > MeleeDistance)
            {
                FinishMelee();
            }
            else
            {
                return; // Keep meleeing
            }
        }

        if (p_hasTarget && p_hasLineOfSight)
        {
            if (m_distanceToTarget <= MeleeDistance)
            {
                // In melee range
                StartMelee();
                m_disengageTimer = DISENGAGE_TIME;
                return;
            }

            if (m_distanceToTarget >= MinDashDistance && m_dashCooldownTimer <= 0f)
            {
                // In dash range and ready
                StartDashWindUp();
                m_disengageTimer = DISENGAGE_TIME;
                return;
            }

            // Normal chase if we aren't in range for abilities
            if (m_currentState != NpcStates.CHASE)
            {
                m_currentState = NpcStates.CHASE;
            }
            m_disengageTimer = DISENGAGE_TIME; // Reset the timer while we have line of sight
        }
        else if (m_currentState == NpcStates.CHASE)
        {
            // We have a target but lost line of sight
            m_disengageTimer -= p_delta;
            if (m_disengageTimer <= 0)
            {
                // Timer expired, return to idle
                m_currentState = NpcStates.IDLE;
                PickNewRandomDirection();
            }
        }
        else
        {
            // IDLE behavior (WANDERING)
            m_currentState = NpcStates.IDLE;
            m_idleTimer -= p_delta;
            if (m_idleTimer <= 0)
            {
                PickNewRandomDirection();
            }
        }
    }

    public void StartMelee()
    {
        m_currentState = LancerStates.MELEE;
        m_currentDirection = Vector2.Zero; // Stop moving
    }

    public void FinishMelee()
    {
        if (m_currentState == NpcStates.DEAD) return;
        m_currentState = NpcStates.IDLE;
    }

    public void StartDashWindUp()
    {
        m_currentState = LancerStates.WIND_UP;
        m_currentDirection = Vector2.Zero; // Stop moving
    }

    public void StartDash()
    {
        m_currentState = LancerStates.DASHING;
        // The direction is set right before transitioning into DASHING
        m_dashCooldownTimer = DashCooldown;
    }

    public void FinishDash()
    {
        if (m_currentState == NpcStates.DEAD) return;
        m_currentState = LancerStates.RECOVERY;
        m_currentDirection = Vector2.Zero; // Stop moving
    }

    public void FinishRecovery()
    {
        if (m_currentState == NpcStates.DEAD) return;
        m_currentState = NpcStates.IDLE;
    }
}
