using Core.Domain;
using Core.Interfaces;

namespace Core.Events;

public record NavigationRejectedEvent(IslandDestination Destination, string Reason) : IEvent;
