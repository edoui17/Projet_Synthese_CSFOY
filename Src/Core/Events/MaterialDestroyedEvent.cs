using Core.Domain;
using Core.Interfaces;

namespace Core.Events;

public record MaterialDestroyedEvent(ResourceItem Item, int MaterialQuantity) : IEvent;
