namespace IslandSurvivor.Interfaces;

public interface INpc
{
    string NpcType { get; }
    string CurrentState { get; }
}
