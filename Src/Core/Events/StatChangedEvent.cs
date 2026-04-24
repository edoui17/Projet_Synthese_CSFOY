using Core.Interfaces;
using Core.Managers.Stats;

namespace Core.Events;

public record StatChangedEvent(StatType StatType, float CurrentValue, float EffectiveMaxValue) : IEvent;
