using Godot;
using IslandSurvivor.Nodes;
using IslandSurvivor.Resources;
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
        void DestroyResource(Core.Domain.DamageContext p_context = null);
    }
}
