using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Interfaces;

namespace Core.Services;

/// <summary>
/// Implementation of the Event Bus using WeakReferences to prevent memory leaks.
/// </summary>
public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<object>> m_subscriptions = new();

    private class WeakAction<T> where T : IEvent
    {
        public WeakReference? TargetReference { get; }
        public MethodInfo Method { get; }

        public WeakAction(Action<T> p_callback)
        {
            if (p_callback.Target != null)
            {
                TargetReference = new WeakReference(p_callback.Target);
            }
            Method = p_callback.Method;
        }

        public bool IsAlive => TargetReference == null || TargetReference.IsAlive;

        public bool IsMatch(Action<T> p_callback)
        {
            if (TargetReference != null)
            {
                return TargetReference.Target == p_callback.Target && Method == p_callback.Method;
            }
            return p_callback.Target == null && Method == p_callback.Method;
        }

        public void Invoke(T p_event)
        {
            if (TargetReference != null)
            {
                object? target = TargetReference.Target;
                if (target != null)
                {
                    Method.Invoke(target, new object[] { p_event });
                }
            }
            else
            {
                // Static method
                Method.Invoke(null, new object[] { p_event });
            }
        }
    }

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
