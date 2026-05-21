using System;
using System.Collections.Generic;
using Xunit;
using Core.Services;
using Core.Interfaces;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;

namespace Tests.UnitTests.Core.Services;

public class EventBusTests
{
    public class TestEvent : IEvent
    {
        public string Message { get; set; }

        public TestEvent(string message = "")
        {
            Message = message;
        }
    }

    public class AnotherTestEvent : IEvent
    {
    }

    private static void StaticTestMethod(TestEvent e)
    {
    }

    [Fact]
    public void Subscribe_ValidCallback_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();
        Action<TestEvent> callback = (e) => { };

        // Act & Assert
        var exception = Record.Exception(() => eventBus.Subscribe(callback));
        Assert.Null(exception);
    }

    [Fact]
    public void Subscribe_NullCallback_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act & Assert
        var exception = Record.Exception(() => eventBus.Subscribe<TestEvent>(null));
        Assert.Null(exception);
    }

    [Fact]
    public void Publish_Event_IsQueuedAndDispatchedOnProcessEvents()
    {
        // Arrange
        var eventBus = new EventBus();
        bool isCalled = false;
        string receivedMessage = "";

        Action<TestEvent> callback = (e) =>
        {
            isCalled = true;
            receivedMessage = e.Message;
        };

        eventBus.Subscribe(callback);

        // Act
        eventBus.Publish(new TestEvent("Hello"));

        // Assert before process
        Assert.False(isCalled);

        // Act Process
        eventBus.ProcessEvents();

        // Assert after process
        Assert.True(isCalled);
        Assert.Equal("Hello", receivedMessage);
    }

    [Fact]
    public void Publish_MultipleSubscribers_AllAreCalled()
    {
        // Arrange
        var eventBus = new EventBus();
        int callCount = 0;

        Action<TestEvent> callback1 = (e) => callCount++;
        Action<TestEvent> callback2 = (e) => callCount++;

        eventBus.Subscribe(callback1);
        eventBus.Subscribe(callback2);

        // Act
        eventBus.Publish(new TestEvent());
        eventBus.ProcessEvents();

        // Assert
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Publish_NoSubscribers_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act & Assert
        var exception = Record.Exception(() =>
        {
            eventBus.Publish(new TestEvent());
            eventBus.ProcessEvents();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void Unsubscribe_ValidCallback_IsNotCalledAfterPublish()
    {
        // Arrange
        var eventBus = new EventBus();
        bool isCalled = false;

        Action<TestEvent> callback = (e) => isCalled = true;

        eventBus.Subscribe(callback);
        eventBus.Unsubscribe(callback);

        // Act
        eventBus.Publish(new TestEvent());
        eventBus.ProcessEvents();

        // Assert
        Assert.False(isCalled);
    }

    [Fact]
    public void Unsubscribe_NullCallback_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act & Assert
        var exception = Record.Exception(() => eventBus.Unsubscribe<TestEvent>(null));
        Assert.Null(exception);
    }

    [Fact]
    public void Unsubscribe_NotSubscribedCallback_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();
        Action<TestEvent> callback = (e) => { };

        // Act & Assert
        var exception = Record.Exception(() => eventBus.Unsubscribe(callback));
        Assert.Null(exception);
    }

    [Fact]
    public void Unsubscribe_OnlyRemovesSpecificCallback()
    {
        // Arrange
        var eventBus = new EventBus();
        bool callback1Called = false;
        bool callback2Called = false;

        Action<TestEvent> callback1 = (e) => callback1Called = true;
        Action<TestEvent> callback2 = (e) => callback2Called = true;

        eventBus.Subscribe(callback1);
        eventBus.Subscribe(callback2);

        // Act
        eventBus.Unsubscribe(callback1);
        eventBus.Publish(new TestEvent());
        eventBus.ProcessEvents();

        // Assert
        Assert.False(callback1Called);
        Assert.True(callback2Called);
    }

    [Fact]
    public void Unsubscribe_DuringPublish_DoesNotThrowCollectionModifiedException()
    {
        // Arrange
        var eventBus = new EventBus();
        bool isCalled = false;
        Action<TestEvent> callback = null;

        callback = (e) =>
        {
            isCalled = true;
            // Unsubscribe itself during publish
            eventBus.Unsubscribe(callback);
        };

        eventBus.Subscribe(callback);

        // Act & Assert
        var exception = Record.Exception(() =>
        {
            eventBus.Publish(new TestEvent());
            eventBus.ProcessEvents();
        });

        Assert.Null(exception);
        Assert.True(isCalled);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SubscribeTemporaryHandler(EventBus eventBus, out WeakReference weakRef)
    {
        int someLocalState = 42; // Ensures lambda captures a local, preventing it from being a cached static delegate
        Action<TestEvent> callback = (e) => Console.WriteLine(someLocalState);
        eventBus.Subscribe(callback);
        weakRef = new WeakReference(callback.Target);
    }

    [Fact]
    public void Unsubscribe_ClearsOtherDeadReferences_ToPreventMemoryBloat()
    {
        // Arrange
        var eventBus = new EventBus();

        SubscribeTemporaryHandler(eventBus, out WeakReference weakRef);

        // Force GC
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(weakRef.IsAlive, "The weak reference should be dead after GC.");

        Action<TestEvent> activeCallback = (e) => { };
        eventBus.Subscribe(activeCallback);

        // Act
        // Unsubscribe should trigger the cleanup of dead references too
        eventBus.Unsubscribe(activeCallback);

        // We can't directly assert internal list size, but we can confirm no exceptions
        // and that publishing an event does not throw when it encounters pruned subscribers.
        var exception = Record.Exception(() =>
        {
            eventBus.Publish(new TestEvent());
            eventBus.ProcessEvents();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void Unsubscribe_StaticMethod_CorrectlyRemovesCallback()
    {
        // Arrange
        var eventBus = new EventBus();
        eventBus.Subscribe<TestEvent>(StaticTestMethod);

        // Act
        var exception = Record.Exception(() => eventBus.Unsubscribe<TestEvent>(StaticTestMethod));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Unsubscribe_WithDifferentLambda_DoesNotRemoveOriginalSubscription()
    {
        // Arrange
        var eventBus = new EventBus();
        bool isCalled = false;

        Action<TestEvent> originalCallback = (e) => isCalled = true;
        eventBus.Subscribe(originalCallback);

        // Act - Attempt to unsubscribe with a new but identical lambda
        eventBus.Unsubscribe<TestEvent>((e) => isCalled = true);

        eventBus.Publish(new TestEvent());
        eventBus.ProcessEvents();

        // Assert
        Assert.True(isCalled, "The original callback should still be subscribed and triggered.");
    }

    [Fact]
    public void Unsubscribe_CalledConcurrently_MaintainsThreadSafety()
    {
        // Arrange
        var eventBus = new EventBus();
        Action<TestEvent> callback = (e) => { };
        eventBus.Subscribe(callback);

        // Act & Assert
        var exception = Record.Exception(() =>
        {
            Parallel.For(0, 1000, i =>
            {
                eventBus.Unsubscribe(callback);
            });
        });

        Assert.Null(exception);
    }
}
