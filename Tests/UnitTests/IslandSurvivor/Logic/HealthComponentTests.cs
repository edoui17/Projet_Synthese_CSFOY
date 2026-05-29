using System;
using Godot;
using Xunit;
using IslandSurvivor.Logic;

namespace Tests.UnitTests.IslandSurvivor.Logic;

public class HealthComponentTests
{
    [Fact]
    public void TakeDamage_ReducesHealth()
    {
        var health = new HealthComponent(10);

        health.TakeDamage(3);

        Assert.Equal(7, health.CurrentHealth);
        Assert.False(health.IsDead);
    }

    [Fact]
    public void TakeDamage_BelowZero_CapsAtZeroAndTriggersDeath()
    {
        var health = new HealthComponent(5);
        bool deathFired = false;
        health.OnDeath += (s, e) => deathFired = true;

        health.TakeDamage(10);

        Assert.Equal(0, health.CurrentHealth);
        Assert.True(health.IsDead);
        Assert.True(deathFired);
    }
}
