using System;
using System.Runtime.CompilerServices;
using Xunit;
using Core.Utils;

namespace UnitTests.Core.Utils;

public class WeakEventTests
{
    private class TestEventArgs : EventArgs
    {
        public int Value { get; set; }
    }

    private class TestSubscriber
    {
        public int CallCount { get; private set; }

        public void HandleEvent(object? sender, TestEventArgs e)
        {
            CallCount++;
        }
    }

    [Fact]
    public void AddListener_InvokesListener_WhenEventIsFired()
    {
        // Arrange
        WeakEvent<TestEventArgs> weakEvent = new WeakEvent<TestEventArgs>();
        TestSubscriber subscriber = new TestSubscriber();
        weakEvent.AddListener(subscriber.HandleEvent);

        // Act
        weakEvent.Invoke(this, new TestEventArgs { Value = 42 });

        // Assert
        Assert.Equal(1, subscriber.CallCount);
    }

    [Fact]
    public void RemoveListener_DoesNotInvokeListener_WhenRemoved()
    {
        // Arrange
        WeakEvent<TestEventArgs> weakEvent = new WeakEvent<TestEventArgs>();
        TestSubscriber subscriber = new TestSubscriber();
        weakEvent.AddListener(subscriber.HandleEvent);
        weakEvent.RemoveListener(subscriber.HandleEvent);

        // Act
        weakEvent.Invoke(this, new TestEventArgs { Value = 42 });

        // Assert
        Assert.Equal(0, subscriber.CallCount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private WeakReference CreateWeakSubscriber(WeakEvent<TestEventArgs> p_weakEvent)
    {
        TestSubscriber subscriber = new TestSubscriber();
        p_weakEvent.AddListener(subscriber.HandleEvent);
        return new WeakReference(subscriber);
    }

    [Fact]
    public void WeakEvent_AllowsSubscriberToBeGarbageCollected()
    {
        // Arrange
        WeakEvent<TestEventArgs> weakEvent = new WeakEvent<TestEventArgs>();
        WeakReference subscriberRef = CreateWeakSubscriber(weakEvent);

        // Act
        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        Assert.False(subscriberRef.IsAlive, "The subscriber should have been garbage collected.");

        // Invoking should not throw an exception even if subscribers are dead
        Exception? exception = Record.Exception(() => weakEvent.Invoke(this, new TestEventArgs { Value = 42 }));
        Assert.Null(exception);
    }
}
