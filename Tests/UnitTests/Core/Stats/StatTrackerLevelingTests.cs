using System.Collections.Generic;
using Core.Managers;
using Core.Events;
using Xunit;
using Core.Services;

namespace UnitTests.Core.Stats;

public class StatTrackerLevelingTests
{
    [Fact]
    public void AddExperience_GainsXpAndTriggersEvent()
    {
        // Arrange
        var eventBus = new EventBus();
        var tracker = new StatTracker(eventBus);
        tracker.InitializeStats(new Dictionary<StatType, float>());

        bool eventTriggered = false;
        eventBus.Subscribe<ExperienceGainedEvent>(e =>
        {
            eventTriggered = true;
            Assert.Equal(10f, e.Amount);
        });

        // Act
        tracker.AddExperience(10f);
        eventBus.ProcessEvents();

        // Assert
        Assert.True(eventTriggered);
        Assert.Equal(10f, tracker.GetCurrentValue(StatType.Experience));
    }

    [Fact]
    public void AddExperience_LevelsUpWhenXpSufficient()
    {
        // Arrange
        var eventBus = new EventBus();
        var tracker = new StatTracker(eventBus);
        tracker.InitializeStats(new Dictionary<StatType, float>());

        bool levelUpTriggered = false;
        eventBus.Subscribe<LevelChangedEvent>(e =>
        {
            levelUpTriggered = true;
            Assert.Equal(2, e.NewLevel);
        });

        // Level 1 required XP is 50 + (1 * 50) = 100
        Assert.Equal(1f, tracker.GetCurrentValue(StatType.Level));

        // Act
        tracker.AddExperience(110f);
        eventBus.ProcessEvents();

        // Assert
        Assert.True(levelUpTriggered);
        Assert.Equal(2f, tracker.GetCurrentValue(StatType.Level));
        // Remaining XP should be 10 (110 - 100)
        Assert.Equal(10f, tracker.GetCurrentValue(StatType.Experience));
    }
}
