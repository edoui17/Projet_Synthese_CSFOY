namespace IslandSurvivor.Logic.Entities;

using System;
using Godot; // Vector2
using Core.Interfaces.Entities;

public class RangedController : AgressorController, IRangedController
{
    private float m_stoppingDistance;

    public float StoppingDistance => m_stoppingDistance;

    public RangedController(float p_stoppingDistance = 200.0f) : base()
    {
        m_stoppingDistance = p_stoppingDistance;
    }

    public void UpdateStoppingDistance(float p_distance)
    {
        m_stoppingDistance = p_distance;
    }
}
