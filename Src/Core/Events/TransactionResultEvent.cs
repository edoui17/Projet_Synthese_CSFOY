using Core.Interfaces;

namespace Core.Events;

public record TransactionResultEvent(string ItemId, bool IsSuccess, string Reason) : IEvent;
