using Godot;
using System;

namespace IslandSurvivor.Interfaces
{
    public interface IGatheringMaterials
    {
        string m_materialName { get; }
        string m_materialType { get; }
        string m_entityId { get; }
        Timer m_timer { get; }

        void OnAreaEntered(Area2D area);
        void DestroyRessource();
    }
}
