using Core.Domain;
using Core.Interfaces;

namespace Core.Events;

public record NavigationApprovedEvent(IslandDestination Destination) : IEvent;
