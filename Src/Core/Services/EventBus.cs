using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Interfaces;
using System.Collections.Concurrent;
using Core.Utils;

namespace Core.Services;

/// <summary>
/// Implementation of the Event Bus using WeakReferences to prevent memory leaks.
/// </summary>
public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<object>> m_subscriptions = new();
    private readonly ConcurrentQueue<Action> m_eventQueue = new();

    public void Subscribe<T>(Action<T> p_callback) where T : IEvent
    {
        if (p_callback == null) return;

        Type eventType = typeof(T);

        lock (m_subscriptions)
        {
            if (!m_subscriptions.TryGetValue(eventType, out var subscribers))
            {
                subscribers = new List<object>();
                m_subscriptions[eventType] = subscribers;
            }

            subscribers.Add(new WeakAction<T>(p_callback));
        }
    }

    public void Unsubscribe<T>(Action<T> p_callback) where T : IEvent
    {
        if (p_callback == null) return;

        Type eventType = typeof(T);

        lock (m_subscriptions)
        {
            if (m_subscriptions.TryGetValue(eventType, out var subscribers))
            {
                subscribers.RemoveAll(p_weakActionObj =>
                {
                    var weakAction = (WeakAction<T>)p_weakActionObj;
                    return weakAction.IsMatch(p_callback) || !weakAction.IsAlive;
                });
            }
        }
    }

    public void Publish<T>(T p_event) where T : IEvent
    {
        m_eventQueue.Enqueue(() => DispatchEvent(p_event));
    }

    public void ProcessEvents()
    {
        while (m_eventQueue.TryDequeue(out var dispatchAction))
        {
            dispatchAction.Invoke();
        }
    }

    private void DispatchEvent<T>(T p_event) where T : IEvent
    {
        Type eventType = typeof(T);
        List<object> subscribersSnapshot = new();

        lock (m_subscriptions)
        {
            if (m_subscriptions.TryGetValue(eventType, out var subscribers))
            {
                // Clean up dead references while taking a snapshot
                List<object> deadReferences = new();
                foreach (var subscriberObj in subscribers)
                {
                    var weakAction = (WeakAction<T>)subscriberObj;
                    if (weakAction.IsAlive)
                    {
                        subscribersSnapshot.Add(subscriberObj);
                    }
                    else
                    {
                        deadReferences.Add(subscriberObj);
                    }
                }

                foreach (var deadRef in deadReferences)
                {
                    subscribers.Remove(deadRef);
                }
            }
        }

        // Invoke outside the lock to prevent deadlocks if a subscriber publishes an event or modifies subscriptions
        foreach (var subscriberObj in subscribersSnapshot)
        {
            var weakAction = (WeakAction<T>)subscriberObj;
            if (weakAction.IsAlive) // Double check in case it died since the snapshot
            {
                weakAction.Invoke(p_event);
            }
        }
    }
}
