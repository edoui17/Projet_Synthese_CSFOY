using System;
using Core.Utils;
using Xunit;

namespace Tests.UnitTests.Core.Utils;

public class SystemRandomProviderTests
{
    [Fact]
    public void Next_WithPositiveMaxValue_ReturnsValueWithinBounds()
    {
        // Arrange
        var provider = new SystemRandomProvider();
        int maxValue = 10;

        // Act
        int result = provider.Next(maxValue);

        // Assert
        Assert.True(result >= 0 && result < maxValue, $"Expected value between 0 and {maxValue - 1}, but got {result}.");
    }

    [Fact]
    public void Next_WithMaxValueZero_ReturnsZero()
    {
        // Arrange
        var provider = new SystemRandomProvider();

        // Act
        int result = provider.Next(0);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Next_WithNegativeMaxValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var provider = new SystemRandomProvider();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => provider.Next(-1));
    }

    [Fact]
    public void NextDouble_ReturnsValueWithinBounds()
    {
        // Arrange
        var provider = new SystemRandomProvider();

        // Act
        double result = provider.NextDouble();

        // Assert
        Assert.True(result >= 0.0 && result < 1.0, $"Expected value between 0.0 and 1.0, but got {result}.");
    }

    [Fact]
    public void Next_CalledMultipleTimes_ProducesValuesWithinBounds()
    {
        // Arrange
        var provider = new SystemRandomProvider();
        int maxValue = 100;
        int iterations = 1000;

        // Act & Assert
        for (int i = 0; i < iterations; i++)
        {
            int result = provider.Next(maxValue);
            Assert.True(result >= 0 && result < maxValue);
        }
    }

    [Fact]
    public void NextDouble_CalledMultipleTimes_ProducesValuesWithinBounds()
    {
        // Arrange
        var provider = new SystemRandomProvider();
        int iterations = 1000;

        // Act & Assert
        for (int i = 0; i < iterations; i++)
        {
            double result = provider.NextDouble();
            Assert.True(result >= 0.0 && result < 1.0);
        }
    }
}
