using Core.Interfaces;
using Core.Managers.Stats;

namespace Core.Events;

public record StatUpgradePurchasedEvent(StatType StatType) : IEvent;
