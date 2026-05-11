using Core.Domain.Models;
using Core.Interfaces;

namespace Core.Events;

public record NavigationApprovedEvent(IslandDestination Destination) : IEvent;
