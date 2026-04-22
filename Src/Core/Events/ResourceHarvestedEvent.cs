using Core.Interfaces;

namespace Core.Events;

public record ResourceHarvestedEvent(string ResourceId, int Amount) : IEvent;
