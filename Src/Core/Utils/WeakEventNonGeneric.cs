namespace Core.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class WeakEvent
{
    private readonly List<WeakDelegate> m_listeners = new List<WeakDelegate>();

    private class WeakDelegate
    {
        public WeakReference? TargetReference { get; }
        public MethodInfo Method { get; }
        public bool IsAlive => TargetReference == null || TargetReference.IsAlive;

        public WeakDelegate(Delegate p_delegate)
        {
            if (p_delegate.Target != null)
            {
                TargetReference = new WeakReference(p_delegate.Target);
            }
            Method = p_delegate.Method;
        }

        public bool IsMatch(Delegate p_delegate)
        {
            if (TargetReference != null)
            {
                return TargetReference.Target == p_delegate.Target && Method == p_delegate.Method;
            }
            return p_delegate.Target == null && Method == p_delegate.Method;
        }

        public void Invoke(object p_sender, EventArgs p_args)
        {
            if (TargetReference != null)
            {
                object? target = TargetReference.Target;
                if (target != null)
                {
                    Method.Invoke(target, new object[] { p_sender, p_args });
                }
            }
            else
            {
                // Static method
                Method.Invoke(null, new object[] { p_sender, p_args });
            }
        }
    }

    public void AddListener(EventHandler p_listener)
    {
        if (p_listener == null) return;
        m_listeners.Add(new WeakDelegate(p_listener));
    }

    public void RemoveListener(EventHandler p_listener)
    {
        if (p_listener == null) return;
        m_listeners.RemoveAll(p_weakDelegate => p_weakDelegate.IsMatch(p_listener) || !p_weakDelegate.IsAlive);
    }

    public void Invoke(object p_sender, EventArgs p_args)
    {
        List<WeakDelegate> toRemove = new List<WeakDelegate>();

        foreach (WeakDelegate weakDelegate in m_listeners.ToList())
        {
            if (weakDelegate.IsAlive)
            {
                weakDelegate.Invoke(p_sender, p_args);
            }
            else
            {
                toRemove.Add(weakDelegate);
            }
        }

        foreach (WeakDelegate deadRef in toRemove)
        {
            m_listeners.Remove(deadRef);
        }
    }
}
