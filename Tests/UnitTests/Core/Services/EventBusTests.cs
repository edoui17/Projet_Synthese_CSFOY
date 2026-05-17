using System;
using System.Collections.Generic;
using Xunit;
using Core.Services;
using Core.Interfaces;

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
}
