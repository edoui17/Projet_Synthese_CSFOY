using Core.Domain;
using Core.Interfaces;

namespace Core.Events;

public record TeleportRequestedEvent(IslandDestination Destination) : IEvent;
