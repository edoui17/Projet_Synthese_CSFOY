namespace IslandSurvivor.Logic.Entities;

using System;
using Godot;
using Core.Interfaces.Entities;

public class AgressorController : IAgressorController
{
    private string m_currentState;
    private float m_idleTimer;
    private float m_disengageTimer;
    private Vector2 m_currentDirection;

    private float m_attackCooldownTimer;
    private float m_attackDurationTimer;

    private readonly Random m_random = new Random();

    public string CurrentState => m_currentState;
    public Vector2 CurrentDirection => m_currentDirection;

    public const float IDLE_DIRECTION_CHANGE_INTERVAL = 2.0f;
    public const float DISENGAGE_TIME = 3.0f;
    public const float ATTACK_COOLDOWN_TIME = 2.0f;
    public const float ATTACK_DURATION_TIME = 0.8f; // Should match or slightly exceed the attack animation length

    public AgressorController()
    {
        m_currentState = NpcStates.IDLE;
        PickNewRandomDirection();
    }

    public void Update(float p_delta, bool p_hasTarget, bool p_hasLineOfSight)
    {
        if (m_currentState == NpcStates.DEAD) return;

        // Decrease cooldown timer
        if (m_attackCooldownTimer > 0)
        {
            m_attackCooldownTimer -= p_delta;
        }

        if (m_currentState == NpcStates.ATTACK)
        {
            m_attackDurationTimer -= p_delta;
            if (m_attackDurationTimer <= 0)
            {
                EndAttack();
                // Re-evaluate state on next frame or fallback to chase immediately
                if (p_hasTarget && p_hasLineOfSight)
                {
                    m_currentState = NpcStates.CHASE;
                    m_disengageTimer = DISENGAGE_TIME;
                }
                else
                {
                    m_currentState = NpcStates.IDLE;
                    PickNewRandomDirection();
                }
            }
            return; // Skip other updates while attacking
        }

        if (p_hasTarget && p_hasLineOfSight)
        {
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
            // IDLE behavior
            m_currentState = NpcStates.IDLE;
            m_idleTimer -= p_delta;
            if (m_idleTimer <= 0)
            {
                PickNewRandomDirection();
            }
        }
    }

    public void UpdateChaseDirection(Vector2 p_agressorPosition, Vector2 p_targetPosition)
    {
        if (m_currentState != NpcStates.CHASE) return;

        Vector2 direction = p_targetPosition - p_agressorPosition;
        if (direction.LengthSquared() > 0)
        {
            m_currentDirection = direction.Normalized();
        }
    }

    public void SetDead()
    {
        m_currentState = NpcStates.DEAD;
        m_currentDirection = Vector2.Zero;
    }

    private void PickNewRandomDirection()
    {
        m_idleTimer = IDLE_DIRECTION_CHANGE_INTERVAL + (float)(m_random.NextDouble() * 2.0 - 1.0); // 1 to 3 seconds

        float angle = (float)(m_random.NextDouble() * Math.PI * 2);
        m_currentDirection = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }

    public void ResetDirectionChangeTimer()
    {
        m_idleTimer = 0;
    }

    public void ForceNewDirection()
    {
        if (m_currentState == NpcStates.IDLE)
        {
            PickNewRandomDirection();
        }
    }

    public bool CanAttack()
    {
        return m_currentState != NpcStates.DEAD && m_currentState != NpcStates.ATTACK && m_attackCooldownTimer <= 0;
    }

    public void StartAttack()
    {
        if (CanAttack())
        {
            m_currentState = NpcStates.ATTACK;
            m_currentDirection = Vector2.Zero;
            m_attackDurationTimer = ATTACK_DURATION_TIME;
            m_attackCooldownTimer = ATTACK_COOLDOWN_TIME;
        }
    }

    public void EndAttack()
    {
        if (m_currentState == NpcStates.ATTACK)
        {
            m_currentState = NpcStates.IDLE;
        }
    }
}
