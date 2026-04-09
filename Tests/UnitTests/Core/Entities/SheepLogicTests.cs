using System;
using System.Numerics;
using Xunit;
using Core.Domain.Entities;

namespace Tests.UnitTests.Core.Entities;

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

public class SheepControllerTests
{
    [Fact]
    public void InitialState_IsIdle()
    {
        var controller = new SheepController();
        Assert.Equal(SheepStates.IDLE, controller.CurrentState);
    }

    [Fact]
    public void StartFleeing_ChangesStateAndSetsDirection()
    {
        var controller = new SheepController();
        var sheepPos = new Vector2(10, 10);
        var attackerPos = new Vector2(0, 10); // Attacker is to the left

        controller.StartFleeing(sheepPos, attackerPos);

        Assert.Equal(SheepStates.FLEE, controller.CurrentState);
        // Should flee to the right (positive X)
        Assert.True(controller.CurrentDirection.X > 0);
        Assert.Equal(0, controller.CurrentDirection.Y);
    }

    [Fact]
    public void Update_FleeTimerExpires_ReturnsToIdle()
    {
        var controller = new SheepController();
        controller.StartFleeing(Vector2.Zero, new Vector2(1, 0));

        // Advance time just before expiration
        controller.Update(SheepController.FLEE_DURATION - 0.1f);
        Assert.Equal(SheepStates.FLEE, controller.CurrentState);

        // Advance past expiration
        controller.Update(0.2f);
        Assert.Equal(SheepStates.IDLE, controller.CurrentState);
    }
}
