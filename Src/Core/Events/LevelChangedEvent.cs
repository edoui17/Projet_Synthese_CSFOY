using Core.Interfaces;

namespace Core.Events;

public class LevelChangedEvent : IEvent
{
    public int NewLevel { get; }

    public LevelChangedEvent(int p_newLevel)
    {
        NewLevel = p_newLevel;
    }
}
