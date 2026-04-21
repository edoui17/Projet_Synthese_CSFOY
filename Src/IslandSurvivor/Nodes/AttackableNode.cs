using Godot;
using IslandSurvivor.Interfaces;

namespace IslandSurvivor.Nodes;

public partial class AttackableNode : Area2D, IAttackable
{
    public void OnAttacked()
    {
        GD.Print($"[COMBAT] {Name} was attacked!");
    }
}
