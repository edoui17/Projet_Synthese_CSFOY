using Godot;
using System;

namespace IslandSurvivor.Interfaces
{
    public interface IGatheringMaterials
    {
        string MaterialName { get; }
        string MaterialType { get; }
        string EntityId { get; }
        Timer Timer { get; }

        void OnAreaEntered(Area2D area);
        void DestroyRessource();
    }
}
