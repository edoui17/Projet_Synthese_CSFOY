namespace IslandSurvivor.Logic.Entities;

using System;
using Godot; // Vector2
using IslandSurvivor.Logic.Entities;

public class AgressorController : IAgressorController
{
    protected string m_currentState;
    protected float m_idleTimer;
    protected float m_disengageTimer;
    protected Vector2 m_currentDirection;
    protected readonly Random m_random = new Random();

    public string CurrentState => m_currentState;
    public Vector2 CurrentDirection => m_currentDirection;
    public const float IDLE_DIRECTION_CHANGE_INTERVAL = 2.0f;
    public const float DISENGAGE_TIME = 3.0f;

    public AgressorController()
    {
        m_currentState = NpcStates.IDLE;
        PickNewRandomDirection();
    }

    public virtual void Update(float p_delta, bool p_hasTarget, bool p_hasLineOfSight)
    {
        if (m_currentState == NpcStates.DEAD) return;

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

    public virtual void UpdateChaseDirection(Vector2 p_agressorPosition, Vector2 p_targetPosition)
    {
        Vector2 direction = p_targetPosition - p_agressorPosition;
        if (direction.LengthSquared() > 0)
        {
            float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            m_currentDirection = new Vector2(direction.X / length, direction.Y / length);
        }
    }

    public virtual void SetDead()
    {
        m_currentState = NpcStates.DEAD;
        m_currentDirection = Vector2.Zero;
    }

    protected virtual void PickNewRandomDirection()
    {
        m_idleTimer = IDLE_DIRECTION_CHANGE_INTERVAL + (float)(m_random.NextDouble() * 2.0 - 1.0); // 1 to 3 seconds

        float angle = (float)(m_random.NextDouble() * Math.PI * 2);
        m_currentDirection = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }

    public virtual void ResetDirectionChangeTimer()
    {
        m_idleTimer = 0;
    }

    public virtual void ForceNewDirection()
    {
        if (m_currentState == NpcStates.IDLE)
        {
            PickNewRandomDirection();
        }
    }
}
