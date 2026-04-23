using Core.Interfaces;

namespace Core.Events;

public record ScoreChangedEvent(int PreviousScore, int NewScore) : IEvent;
