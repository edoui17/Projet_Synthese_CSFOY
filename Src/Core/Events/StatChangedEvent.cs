using Core.Interfaces;
using Core.Managers;

namespace Core.Events;

public record StatChangedEvent(StatType StatType, float CurrentValue, float EffectiveMaxValue) : IEvent;
