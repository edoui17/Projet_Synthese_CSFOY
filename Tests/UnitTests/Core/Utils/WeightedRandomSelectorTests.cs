using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces.Utils;
using Core.Utils;
using Xunit;
using Moq;

namespace UnitTests.Core.Utils;

public class WeightedRandomSelectorTests
{
    private class TestItem : IWeightedItem
    {
        public string Name { get; set; }
        public float Weight { get; set; }
    }

    [Fact]
    public void Constructor_NullRandomProvider_ThrowsArgumentNullException()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new WeightedRandomSelector<TestItem>(null!));
    }

    [Fact]
    public void SelectRandom_NullList_ReturnsDefault()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        var selector = new WeightedRandomSelector<TestItem>(mockRandom.Object);

        // Act
        var result = selector.SelectRandom(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectRandom_EmptyList_ReturnsDefault()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        var selector = new WeightedRandomSelector<TestItem>(mockRandom.Object);
        var items = new List<TestItem>();

        // Act
        var result = selector.SelectRandom(items);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectRandom_ValidWeights_SelectsCorrectItemBasedOnRoll()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        // Total weight is 10. ItemA covers 0.0-2.0, ItemB covers 2.0-7.0, ItemC covers 7.0-10.0
        mockRandom.Setup(r => r.NextDouble()).Returns(0.5); // 0.5 * 10 = 5.0 -> Falls into ItemB's range

        var selector = new WeightedRandomSelector<TestItem>(mockRandom.Object);
        var items = new List<TestItem>
        {
            new TestItem { Name = "ItemA", Weight = 2.0f },
            new TestItem { Name = "ItemB", Weight = 5.0f },
            new TestItem { Name = "ItemC", Weight = 3.0f }
        };

        // Act
        var result = selector.SelectRandom(items);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ItemB", result!.Name);
    }

    [Fact]
    public void SelectRandom_LastBoundary_SelectsLastItem()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        // Total weight is 10. Rolling 0.99 means 9.9, which hits ItemC
        mockRandom.Setup(r => r.NextDouble()).Returns(0.99);

        var selector = new WeightedRandomSelector<TestItem>(mockRandom.Object);
        var items = new List<TestItem>
        {
            new TestItem { Name = "ItemA", Weight = 2.0f },
            new TestItem { Name = "ItemB", Weight = 5.0f },
            new TestItem { Name = "ItemC", Weight = 3.0f }
        };

        // Act
        var result = selector.SelectRandom(items);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ItemC", result!.Name);
    }

    [Fact]
    public void SelectRandom_ZeroTotalWeight_PicksUniformly()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        // With total weight 0, it calls Next(list.Count). Let's mock it to return index 1 (ItemB)
        mockRandom.Setup(r => r.Next(3)).Returns(1);

        var selector = new WeightedRandomSelector<TestItem>(mockRandom.Object);
        var items = new List<TestItem>
        {
            new TestItem { Name = "ItemA", Weight = 0f },
            new TestItem { Name = "ItemB", Weight = 0f },
            new TestItem { Name = "ItemC", Weight = 0f }
        };

        // Act
        var result = selector.SelectRandom(items);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ItemB", result!.Name);
    }

    [Fact]
    public void SelectRandom_NegativeWeights_ClampsAndLogsWarning()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        // Weight sum will be: Math.Max(0, -2) + Math.Max(0, 5) = 5.
        // If roll is 0.5, target is 2.5, which falls on ItemB.
        mockRandom.Setup(r => r.NextDouble()).Returns(0.5);

        var mockLogger = new Mock<ILogger>();
        var selector = new WeightedRandomSelector<TestItem>(mockRandom.Object, mockLogger.Object);

        var items = new List<TestItem>
        {
            new TestItem { Name = "ItemA", Weight = -2.0f },
            new TestItem { Name = "ItemB", Weight = 5.0f }
        };

        // Act
        var result = selector.SelectRandom(items);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ItemB", result!.Name);

        // Verify logger was called
        mockLogger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Negative weight -2"))), Times.Once);
    }
}
