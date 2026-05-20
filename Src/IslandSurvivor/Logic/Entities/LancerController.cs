namespace IslandSurvivor.Logic.Entities;

using System;
using Godot;

public class LancerController : AgressorController, ILancerController
{
    public float MinDashDistance { get; } = 250f;
    public float MeleeDistance { get; } = 120f;
    public float DashCooldown { get; } = 3.0f; // Seconds between dashes

    private float m_distanceToTarget = float.MaxValue;
    private float m_dashCooldownTimer = 0f;
    private Vector2 m_agressorPos = Vector2.Zero;
    private Vector2 m_targetPos = Vector2.Zero;

    // Tolerance for how closely aligned the Y axis must be to trigger a dash
    public const float DASH_Y_ALIGNMENT_TOLERANCE = 20f;

    public LancerController() : base()
    {
    }

    public void UpdateDistanceToTarget(float p_distance)
    {
        m_distanceToTarget = p_distance;
    }

    public void UpdateTargetPositions(Vector2 p_agressorPos, Vector2 p_targetPos)
    {
        m_agressorPos = p_agressorPos;
        m_targetPos = p_targetPos;
    }

    public override void UpdateChaseDirection(Vector2 p_agressorPosition, Vector2 p_targetPosition)
    {
        Vector2 direction = p_targetPosition - p_agressorPosition;
        if (direction.LengthSquared() > 0)
        {
            float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            if (m_currentState == LancerStates.DASHING || m_currentState == LancerStates.WIND_UP)
            {
                // Force pure horizontal dash
                m_currentDirection = new Vector2(direction.X > 0 ? 1f : -1f, 0f);
            }
            else if (m_currentState == NpcStates.CHASE && m_dashCooldownTimer <= 0f && m_distanceToTarget >= MinDashDistance)
            {
                // Dash is ready, but we are in Chase because we need Y alignment
                // Prioritize moving on the Y axis to line up for the dash
                if (Math.Abs(direction.Y) > DASH_Y_ALIGNMENT_TOLERANCE)
                {
                    // Give Y movement much higher weight
                    m_currentDirection = new Vector2(direction.X / length * 0.2f, direction.Y > 0 ? 1f : -1f).Normalized();
                }
                else
                {
                    // Once aligned, normal chase direction
                    m_currentDirection = new Vector2(direction.X / length, direction.Y / length);
                }
            }
            else
            {
                m_currentDirection = new Vector2(direction.X / length, direction.Y / length);
            }
        }
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
                // Must be vertically aligned to dash
                if (Math.Abs(m_targetPos.Y - m_agressorPos.Y) <= DASH_Y_ALIGNMENT_TOLERANCE)
                {
                    // In dash range, ready, and aligned on Y axis
                    StartDashWindUp();
                    m_disengageTimer = DISENGAGE_TIME;
                    return;
                }
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
