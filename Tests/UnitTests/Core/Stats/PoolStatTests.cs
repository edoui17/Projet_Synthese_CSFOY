using Core.Events;
using Core.Managers;
using Core.Services;
using Xunit;

namespace UnitTests.Core.Stats;

public class PoolStatTests
{
    [Fact]
    public void ModifyCurrentValue_WithinBounds_UpdatesValue()
    {
        // Arrange
        var eventBus = new EventBus();
        var poolStat = new PoolStat(StatType.Health, 100f, eventBus);

        // Value starts at 100, we lower it first to test an increase.
        poolStat.SetCurrentValue(50f);

        // Act
        poolStat.ModifyCurrentValue(25f);

        // Assert
        Assert.Equal(75f, poolStat.CurrentValue);
    }

    [Fact]
    public void ModifyCurrentValue_DecreaseBelowZero_ClampsAtZero()
    {
        // Arrange
        var eventBus = new EventBus();
        var poolStat = new PoolStat(StatType.Health, 100f, eventBus);

        // Act
        poolStat.ModifyCurrentValue(-150f);

        // Assert
        Assert.Equal(0f, poolStat.CurrentValue);
    }

    [Fact]
    public void ModifyCurrentValue_IncreaseAboveMax_ClampsAtEffectiveMaxValue()
    {
        // Arrange
        var eventBus = new EventBus();
        var poolStat = new PoolStat(StatType.Health, 100f, eventBus);

        // Start at 100 (EffectiveMaxValue), let's set it to 50
        poolStat.SetCurrentValue(50f);

        // Act
        poolStat.ModifyCurrentValue(100f);

        // Assert
        Assert.Equal(100f, poolStat.CurrentValue);
        Assert.Equal(100f, poolStat.EffectiveMaxValue);
    }

    [Fact]
    public void ModifyCurrentValue_PublishesStatChangedEvent()
    {
        // Arrange
        var eventBus = new EventBus();
        var poolStat = new PoolStat(StatType.Health, 100f, eventBus);

        bool eventTriggered = false;
        eventBus.Subscribe<StatChangedEvent>(e =>
        {
            if (e.StatType == StatType.Health)
            {
                eventTriggered = true;
                Assert.Equal(80f, e.CurrentValue);
                Assert.Equal(100f, e.EffectiveMaxValue);
            }
        });

        // Act
        poolStat.ModifyCurrentValue(-20f);
        eventBus.ProcessEvents();

        // Assert
        Assert.True(eventTriggered);
    }
}
