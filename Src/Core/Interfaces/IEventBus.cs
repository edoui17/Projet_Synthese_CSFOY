using System;

namespace Core.Interfaces;

/// <summary>
/// Interface for a generic Event Bus enabling decoupled communication.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Subscribes a callback to a specific event type.
    /// </summary>
    void Subscribe<T>(Action<T> p_callback) where T : IEvent;

    /// <summary>
    /// Unsubscribes a callback from a specific event type.
    /// </summary>
    void Unsubscribe<T>(Action<T> p_callback) where T : IEvent;

    /// <summary>
    /// Enqueues an event to be published during the next ProcessEvents call.
    /// </summary>
    void Publish<T>(T p_event) where T : IEvent;

    /// <summary>
    /// Processes and dispatches all queued events to their subscribers.
    /// </summary>
    void ProcessEvents();
}
