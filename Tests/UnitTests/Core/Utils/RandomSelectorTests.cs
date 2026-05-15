using System.Collections.Generic;
using Core.Utils;
using Xunit;

namespace UnitTests.Core.Utils;

public class RandomSelectorTests
{
    [Fact]
    public void SelectRandom_ReturnsItemFromList()
    {
        // Arrange
        var selector = new RandomSelector<string>();
        var items = new List<string> { "A", "B", "C" };

        // Act
        var result = selector.SelectRandom(items);

        // Assert
        Assert.Contains(result, items);
    }

    [Fact]
    public void SelectRandom_EmptyList_ReturnsDefault()
    {
        // Arrange
        var selector = new RandomSelector<string>();
        var items = new List<string>();

        // Act
        var result = selector.SelectRandom(items);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectRandom_NullList_ReturnsDefault()
    {
        // Arrange
        var selector = new RandomSelector<string>();

        // Act
        var result = selector.SelectRandom(null!);

        // Assert
        Assert.Null(result);
    }
}
