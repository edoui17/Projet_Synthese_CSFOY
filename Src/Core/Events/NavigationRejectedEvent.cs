using Core.Domain.Models;
using Core.Interfaces;

namespace Core.Events;

public record NavigationRejectedEvent(IslandDestination Destination, string Reason) : IEvent;
