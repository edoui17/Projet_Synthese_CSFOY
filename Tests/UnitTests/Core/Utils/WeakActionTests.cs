using System;
using System.Runtime.CompilerServices;
using Core.Interfaces;
using Core.Utils;
using Xunit;

namespace UnitTests.Core.Utils;

public class WeakActionTests
{
    // Dummy event for testing
    public class TestEvent : IEvent
    {
        public string Message { get; set; } = string.Empty;
    }

    // Dummy target class for instance methods
    private class DummyTarget
    {
        public bool EventReceived { get; private set; }
        public string ReceivedMessage { get; private set; } = string.Empty;

        public void HandleEvent(TestEvent p_event)
        {
            EventReceived = true;
            ReceivedMessage = p_event.Message;
        }
    }

    // Static event handler flag
    private static bool s_staticEventReceived;
    private static string s_staticReceivedMessage = string.Empty;

    private static void StaticHandleEvent(TestEvent p_event)
    {
        s_staticEventReceived = true;
        s_staticReceivedMessage = p_event.Message;
    }

    public WeakActionTests()
    {
        // Reset static state before each test
        s_staticEventReceived = false;
        s_staticReceivedMessage = string.Empty;
    }

    [Fact]
    public void Constructor_WithInstanceMethod_ShouldSetTargetReference()
    {
        // Arrange
        var target = new DummyTarget();
        Action<TestEvent> action = target.HandleEvent;

        // Act
        var weakAction = new WeakAction<TestEvent>(action);

        // Assert
        Assert.NotNull(weakAction.TargetReference);
        Assert.True(weakAction.IsAlive);
        Assert.Equal(action.Method, weakAction.Method);
    }

    [Fact]
    public void Constructor_WithStaticMethod_ShouldNotSetTargetReference()
    {
        // Arrange
        Action<TestEvent> action = StaticHandleEvent;

        // Act
        var weakAction = new WeakAction<TestEvent>(action);

        // Assert
        Assert.Null(weakAction.TargetReference);
        Assert.True(weakAction.IsAlive); // Static methods should always be considered "alive"
        Assert.Equal(action.Method, weakAction.Method);
    }

    [Fact]
    public void IsMatch_WithSameInstanceMethod_ShouldReturnTrue()
    {
        // Arrange
        var target = new DummyTarget();
        var weakAction = new WeakAction<TestEvent>(target.HandleEvent);

        // Act & Assert
        Assert.True(weakAction.IsMatch(target.HandleEvent));
    }

    [Fact]
    public void IsMatch_WithDifferentInstanceMethod_ShouldReturnFalse()
    {
        // Arrange
        var target1 = new DummyTarget();
        var target2 = new DummyTarget();
        var weakAction = new WeakAction<TestEvent>(target1.HandleEvent);

        // Act & Assert
        Assert.False(weakAction.IsMatch(target2.HandleEvent));
    }

    [Fact]
    public void IsMatch_WithSameStaticMethod_ShouldReturnTrue()
    {
        // Arrange
        var weakAction = new WeakAction<TestEvent>(StaticHandleEvent);

        // Act & Assert
        Assert.True(weakAction.IsMatch(StaticHandleEvent));
    }

    [Fact]
    public void Invoke_WithAliveInstance_ShouldExecuteMethod()
    {
        // Arrange
        var target = new DummyTarget();
        var weakAction = new WeakAction<TestEvent>(target.HandleEvent);
        var testEvent = new TestEvent { Message = "Hello Instance" };

        // Act
        weakAction.Invoke(testEvent);

        // Assert
        Assert.True(target.EventReceived);
        Assert.Equal("Hello Instance", target.ReceivedMessage);
    }

    [Fact]
    public void Invoke_WithStaticMethod_ShouldExecuteMethod()
    {
        // Arrange
        var weakAction = new WeakAction<TestEvent>(StaticHandleEvent);
        var testEvent = new TestEvent { Message = "Hello Static" };

        // Act
        weakAction.Invoke(testEvent);

        // Assert
        Assert.True(s_staticEventReceived);
        Assert.Equal("Hello Static", s_staticReceivedMessage);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private WeakAction<TestEvent> CreateWeakActionAndDropReference()
    {
        var target = new DummyTarget();
        return new WeakAction<TestEvent>(target.HandleEvent);
        // target goes out of scope here
    }

    [Fact]
    public void IsAlive_AfterTargetGarbageCollected_ShouldReturnFalse()
    {
        // Arrange
        var weakAction = CreateWeakActionAndDropReference();

        // Act
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Assert
        Assert.False(weakAction.IsAlive);
    }

    [Fact]
    public void Invoke_AfterTargetGarbageCollected_ShouldNotThrow()
    {
        // Arrange
        var weakAction = CreateWeakActionAndDropReference();
        var testEvent = new TestEvent { Message = "Ghost Message" };

        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Act & Assert
        // Should not throw an exception, should just quietly do nothing
        var exception = Record.Exception(() => weakAction.Invoke(testEvent));
        Assert.Null(exception);
    }
}
