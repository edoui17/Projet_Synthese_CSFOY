using System.Collections.Generic;
using Core.Managers.Stats;
using Xunit;

namespace UnitTests.Core.Stats;

public class StatTrackerTests
{
    [Fact]
    public void InitializeStats_SetsValuesCorrectly()
    {
        // Arrange
        StatTracker tracker = new StatTracker();
        Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>
        {
            { StatType.Health, 100f },
            { StatType.Attack, 10f }
        };

        // Act
        tracker.InitializeStats(baseStats);

        // Assert
        Assert.Equal(100f, tracker.GetCurrentValue(StatType.Health));
        Assert.Equal(100f, tracker.GetEffectiveMaxValue(StatType.Health));
        Assert.Equal(10f, tracker.GetCurrentValue(StatType.Attack));
    }

    [Fact]
    public void ModifyCurrentValue_ClampsAtZeroAndMax()
    {
        // Arrange
        StatTracker tracker = new StatTracker();
        Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>
        {
            { StatType.Health, 100f }
        };
        tracker.InitializeStats(baseStats);

        // Act - Damage below 0
        tracker.ModifyCurrentValue(StatType.Health, -150f);

        // Assert - Clamp at 0
        Assert.Equal(0f, tracker.GetCurrentValue(StatType.Health));

        // Act - Heal above max
        tracker.ModifyCurrentValue(StatType.Health, 200f);

        // Assert - Clamp at Max
        Assert.Equal(100f, tracker.GetCurrentValue(StatType.Health));
    }

    [Fact]
    public void AddPermanentBonus_UpdatesMaxAndCurrentSimultaneously()
    {
        // Arrange
        StatTracker tracker = new StatTracker();
        Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>
        {
            { StatType.Attack, 10f }
        };
        tracker.InitializeStats(baseStats);

        // Act - Add permanent +5
        tracker.AddPermanentBonus(StatType.Attack, 5f);

        // Assert
        Assert.Equal(15f, tracker.GetEffectiveMaxValue(StatType.Attack));
        Assert.Equal(15f, tracker.GetCurrentValue(StatType.Attack)); // According to Scenario 4, Current adjusts with Max
    }

    [Fact]
    public void Events_TriggerOnModifyAndBonus()
    {
        // Arrange
        StatTracker tracker = new StatTracker();
        Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>
        {
            { StatType.Health, 100f }
        };
        tracker.InitializeStats(baseStats);

        bool eventTriggered = false;
        tracker.OnAnyStatChanged.AddListener((sender, args) =>
        {
            if (args.StatType == StatType.Health)
            {
                eventTriggered = true;
                Assert.Equal(80f, args.CurrentValue);
                Assert.Equal(100f, args.EffectiveMaxValue);
            }
        });

        // Act
        tracker.ModifyCurrentValue(StatType.Health, -20f);

        // Assert
        Assert.True(eventTriggered);
    }
}
