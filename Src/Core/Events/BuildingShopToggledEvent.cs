using Core.Interfaces;

namespace Core.Events;

public record BuildingShopToggledEvent(bool IsOpen, string BuildingId) : IEvent;
