using Core.Events;
using Core.Interfaces;
using Core.Managers;
using Moq;
using Xunit;

namespace UnitTests.Core.Stats;

public class AttributeStatTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var statType = StatType.Attack;
        var baseValue = 10f;

        // Act
        var stat = new AttributeStat(statType, baseValue, mockEventBus.Object);

        // Assert
        Assert.Equal(statType, stat.StatType);
        Assert.Equal(baseValue, stat.BaseValue);
        Assert.Equal(0f, stat.AdditionalValue);
        Assert.Equal(baseValue, stat.EffectiveMaxValue);
        Assert.Equal(baseValue, stat.CurrentValue);
    }

    [Fact]
    public void AddBonus_PositiveAmount_IncreasesAdditionalValueAndEffectiveMaxValue()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var stat = new AttributeStat(StatType.Attack, 10f, mockEventBus.Object);
        var bonusAmount = 5f;

        // Act
        stat.AddBonus(bonusAmount);

        // Assert
        Assert.Equal(10f, stat.BaseValue);
        Assert.Equal(bonusAmount, stat.AdditionalValue);
        Assert.Equal(15f, stat.EffectiveMaxValue);
        Assert.Equal(15f, stat.CurrentValue);

        mockEventBus.Verify(eb => eb.Publish(It.Is<StatChangedEvent>(e =>
            e.StatType == StatType.Attack &&
            e.CurrentValue == 15f &&
            e.EffectiveMaxValue == 15f)), Times.Once);
    }

    [Fact]
    public void AddBonus_NegativeAmount_DecreasesAdditionalValueAndEffectiveMaxValue()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var stat = new AttributeStat(StatType.Attack, 10f, mockEventBus.Object);
        var bonusAmount = -3f;

        // Act
        stat.AddBonus(bonusAmount);

        // Assert
        Assert.Equal(10f, stat.BaseValue);
        Assert.Equal(bonusAmount, stat.AdditionalValue);
        Assert.Equal(7f, stat.EffectiveMaxValue);
        Assert.Equal(7f, stat.CurrentValue);

        mockEventBus.Verify(eb => eb.Publish(It.Is<StatChangedEvent>(e =>
            e.StatType == StatType.Attack &&
            e.CurrentValue == 7f &&
            e.EffectiveMaxValue == 7f)), Times.Once);
    }

    [Fact]
    public void AddBonus_ZeroAmount_DoesNotChangeValueButTriggersEvent()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var stat = new AttributeStat(StatType.Attack, 10f, mockEventBus.Object);

        // Act
        stat.AddBonus(0f);

        // Assert
        Assert.Equal(10f, stat.BaseValue);
        Assert.Equal(0f, stat.AdditionalValue);
        Assert.Equal(10f, stat.EffectiveMaxValue);
        Assert.Equal(10f, stat.CurrentValue);

        mockEventBus.Verify(eb => eb.Publish(It.Is<StatChangedEvent>(e =>
            e.StatType == StatType.Attack &&
            e.CurrentValue == 10f &&
            e.EffectiveMaxValue == 10f)), Times.Once);
    }

    [Fact]
    public void ModifyCurrentValue_ActsAsNoOp_ValuesRemainUnchanged()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var stat = new AttributeStat(StatType.Attack, 10f, mockEventBus.Object);

        // Act
        stat.ModifyCurrentValue(-5f);

        // Assert
        Assert.Equal(10f, stat.BaseValue);
        Assert.Equal(0f, stat.AdditionalValue);
        Assert.Equal(10f, stat.EffectiveMaxValue);
        Assert.Equal(10f, stat.CurrentValue);

        // Ensure no event was published because it's a no-op
        mockEventBus.Verify(eb => eb.Publish(It.IsAny<StatChangedEvent>()), Times.Never);
    }

    [Fact]
    public void SetCurrentValue_UpdatesBaseValueAndTriggersEvent()
    {
        // Arrange
        var mockEventBus = new Mock<IEventBus>();
        var stat = new AttributeStat(StatType.Attack, 10f, mockEventBus.Object);
        var newBaseValue = 20f;

        // Act
        stat.SetCurrentValue(newBaseValue);

        // Assert
        Assert.Equal(newBaseValue, stat.BaseValue);
        Assert.Equal(0f, stat.AdditionalValue);
        Assert.Equal(newBaseValue, stat.EffectiveMaxValue);
        Assert.Equal(newBaseValue, stat.CurrentValue);

        mockEventBus.Verify(eb => eb.Publish(It.Is<StatChangedEvent>(e =>
            e.StatType == StatType.Attack &&
            e.CurrentValue == newBaseValue &&
            e.EffectiveMaxValue == newBaseValue)), Times.Once);
    }
}
