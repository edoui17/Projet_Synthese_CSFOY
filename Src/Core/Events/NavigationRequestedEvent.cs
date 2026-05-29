using Core.Domain;
using Core.Interfaces;

namespace Core.Events;

public record NavigationRequestedEvent(IslandDestination Destination) : IEvent;
