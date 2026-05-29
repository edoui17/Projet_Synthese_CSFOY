using Core.Interfaces;
using Core.Managers;

namespace Core.Events;

public record StatUpgradePurchasedEvent(StatType StatType) : IEvent;
