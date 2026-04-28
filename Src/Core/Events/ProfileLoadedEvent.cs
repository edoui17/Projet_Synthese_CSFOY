using Core.Domain;
using Core.Interfaces;

namespace Core.Events;

public record ProfileLoadedEvent(PlayerProfile Profile) : IEvent;
