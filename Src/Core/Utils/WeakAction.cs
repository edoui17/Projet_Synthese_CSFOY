using System;
using System.Reflection;
using Core.Interfaces;

namespace Core.Utils;

/// <summary>
/// A weak reference wrapper for delegates to prevent memory leaks in the EventBus.
/// </summary>
public class WeakAction<T> where T : IEvent
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
