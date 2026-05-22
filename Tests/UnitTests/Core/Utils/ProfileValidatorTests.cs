using Xunit;
using Core.Domain;
using Core.Utils;
using System.Collections.Generic;

namespace Tests.UnitTests.Core.Utils;

public class ProfileValidatorTests
{
    [Fact]
    public void IsValid_ValidProfile_ReturnsTrue()
    {
        var profile = new ProfileResponse
        {
            LastSessions = new List<GameStats>
            {
                new GameStats { Health = 50, Attack = 10, Speed = 10, Luck = 5 }
            }
        };

        Assert.True(ProfileValidator.IsValid(profile));
    }

    [Fact]
    public void IsValid_NullProfile_ReturnsFalse()
    {
        Assert.False(ProfileValidator.IsValid(null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void IsValid_InvalidHealth_ReturnsFalse(float health)
    {
        var profile = new ProfileResponse
        {
            LastSessions = new List<GameStats>
            {
                new GameStats { Health = health, Attack = 10, Speed = 10, Luck = 5 }
            }
        };

        Assert.False(ProfileValidator.IsValid(profile));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void IsValid_InvalidAttack_ReturnsFalse(float attack)
    {
        var profile = new ProfileResponse
        {
            LastSessions = new List<GameStats>
            {
                new GameStats { Health = 50, Attack = attack, Speed = 10, Luck = 5 }
            }
        };

        Assert.False(ProfileValidator.IsValid(profile));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void IsValid_InvalidSpeed_ReturnsFalse(float speed)
    {
        var profile = new ProfileResponse
        {
            LastSessions = new List<GameStats>
            {
                new GameStats { Health = 50, Attack = 10, Speed = speed, Luck = 5 }
            }
        };

        Assert.False(ProfileValidator.IsValid(profile));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void IsValid_InvalidLuck_ReturnsFalse(float luck)
    {
        var profile = new ProfileResponse
        {
            LastSessions = new List<GameStats>
            {
                new GameStats { Health = 50, Attack = 10, Speed = 10, Luck = luck }
            }
        };

        Assert.False(ProfileValidator.IsValid(profile));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void IsValid_InvalidBonusStats_ReturnsFalse(float bonus)
    {
        var profile = new ProfileResponse
        {
            LastSessions = new List<GameStats>
            {
                new GameStats { Health = 50, Attack = 10, Speed = 10, Luck = 5, BonusHealth = bonus }
            }
        };

        Assert.False(ProfileValidator.IsValid(profile));
    }
}
