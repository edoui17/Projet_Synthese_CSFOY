using Core.Domain;
using Core.Services;
using Xunit;
using System;

namespace Tests.UnitTests.Core;

public class ProgressionServiceTests
{
    private readonly ProgressionService m_progressionService;

    public ProgressionServiceTests()
    {
        m_progressionService = new ProgressionService();
    }

    [Theory]
    [InlineData(int.MinValue, 1)]
    [InlineData(-100, 1)]
    [InlineData(0, 1)]
    [InlineData(500, 1)]
    [InlineData(999, 1)]
    [InlineData(1000, 2)]
    [InlineData(2499, 2)]
    [InlineData(2500, 3)]
    [InlineData(4999, 3)]
    [InlineData(5000, 4)]
    [InlineData(9999, 4)]
    [InlineData(10000, 5)]
    [InlineData(15000, 6)]
    [InlineData(500000, 103)]
    [InlineData(int.MaxValue, 429499)]
    public void CalculateLevel_ReturnsCorrectLevel(int p_score, int p_expectedLevel)
    {
        int actualLevel = m_progressionService.CalculateLevel(p_score);
        Assert.Equal(p_expectedLevel, actualLevel);
    }

    [Fact]
    public void CheckAndUpdateHighScore_ReturnsFalseWhenPlayerIsNull()
    {
        bool updated = m_progressionService.CheckAndUpdateHighScore(null!, 1500);
        Assert.False(updated);
    }

    [Fact]
    public void CheckAndUpdateHighScore_UpdatesWhenHigher()
    {
        global::Core.Domain.Player player = new global::Core.Domain.Player { HighScore = 1000 };
        bool updated = m_progressionService.CheckAndUpdateHighScore(player, 1500);

        Assert.True(updated);
        Assert.Equal(1500, player.HighScore);
    }

    [Fact]
    public void CheckAndUpdateHighScore_DoesNotUpdateWhenLowerOrEqual()
    {
        global::Core.Domain.Player player = new global::Core.Domain.Player { HighScore = 1000 };

        bool updatedLower = m_progressionService.CheckAndUpdateHighScore(player, 500);
        Assert.False(updatedLower);
        Assert.Equal(1000, player.HighScore);

        bool updatedEqual = m_progressionService.CheckAndUpdateHighScore(player, 1000);
        Assert.False(updatedEqual);
        Assert.Equal(1000, player.HighScore);
    }
}
