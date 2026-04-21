namespace IslandSurvivor.Logic.Entities;

using System;
using Godot;

public class SheepController
{
    private string m_currentState;
    private float m_fleeTimer;
    private float m_idleTimer;
    private Vector2 m_currentDirection;

    private readonly Random m_random = new Random();

    public string CurrentState => m_currentState;
    public Vector2 CurrentDirection => m_currentDirection;

    // Configuration constants
    public const float FLEE_DURATION = 3.0f;
    public const float IDLE_DIRECTION_CHANGE_INTERVAL = 2.0f;

    public SheepController()
    {
        m_currentState = SheepStates.IDLE;
        PickNewRandomDirection();
    }

    public void Update(float p_delta)
    {
        if (m_currentState == SheepStates.DEAD) return;

        if (m_currentState == SheepStates.FLEE)
        {
            m_fleeTimer -= p_delta;
            if (m_fleeTimer <= 0)
            {
                m_currentState = SheepStates.IDLE;
                PickNewRandomDirection();
            }
        }
        else if (m_currentState == SheepStates.IDLE)
        {
            m_idleTimer -= p_delta;
            if (m_idleTimer <= 0)
            {
                PickNewRandomDirection();
            }
        }
    }

    public void StartFleeing(Vector2 p_sheepPosition, Vector2 p_attackerPosition)
    {
        m_currentState = SheepStates.FLEE;
        m_fleeTimer = FLEE_DURATION;

        // Calculate direction opposite to attacker
        Vector2 direction = p_sheepPosition - p_attackerPosition;

        if (direction.LengthSquared() > 0)
        {
            m_currentDirection = direction.Normalized();
        }
        else
        {
            // If positions are exactly the same, pick random to avoid zero vector
            PickNewRandomDirection();
        }
    }

    public void SetDead()
    {
        m_currentState = SheepStates.DEAD;
        m_currentDirection = Vector2.Zero;
    }

    private void PickNewRandomDirection()
    {
        m_idleTimer = IDLE_DIRECTION_CHANGE_INTERVAL + (float)(m_random.NextDouble() * 2.0 - 1.0); // 1 to 3 seconds

        float angle = (float)(m_random.NextDouble() * Math.PI * 2);
        m_currentDirection = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }
}
