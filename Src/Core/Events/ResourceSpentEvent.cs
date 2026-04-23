using Core.Interfaces;

namespace Core.Events;

public record ResourceSpentEvent(string ResourceId, int Amount) : IEvent;
