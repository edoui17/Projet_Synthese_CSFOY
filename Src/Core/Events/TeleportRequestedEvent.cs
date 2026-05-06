using Core.Domain.Models;
using Core.Interfaces;

namespace Core.Events;

public record TeleportRequestedEvent(IslandDestination Destination) : IEvent;
