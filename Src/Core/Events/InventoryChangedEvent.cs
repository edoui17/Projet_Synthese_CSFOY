using Core.Interfaces;

namespace Core.Events;

public record InventoryChangedEvent(string ResourceId, int TotalAmount) : IEvent;
