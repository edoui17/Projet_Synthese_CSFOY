using Core.Interfaces;

namespace Core.Events;

public record SyncFailedEvent(string Reason) : IEvent;
