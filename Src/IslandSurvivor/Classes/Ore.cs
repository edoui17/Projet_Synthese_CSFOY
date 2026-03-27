using Godot;
using System;

namespace IslandSurvivor.Classes
{
    public interface Ore
    {
        string m_entityId { get; }
        Timer m_timer { get; }

        void OnAreaEntered(Area2D area);
        void DestroyOre();
    }
}
