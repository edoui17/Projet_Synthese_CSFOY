using Godot;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
using System;

namespace IslandSurvivor.Interfaces
{
    public interface IGatheringMaterials
    {
        StatManager Stats { get; }
        string MaterialName { get; }
        string MaterialType { get; }
        string EntityId { get; }
        Timer Timer { get; }

        void OnAreaEntered(Area2D area);
        void DestroyResource(object? p_attacker = null);
    }
}
