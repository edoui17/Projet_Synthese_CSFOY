using Core.Interfaces;

namespace Core.Events;

public record PurchaseAttemptedEvent(string ItemId, int Cost) : IEvent;
