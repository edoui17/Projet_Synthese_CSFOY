using System;
using Godot;
using Xunit;
using IslandSurvivor.Logic.Entities;

namespace Tests.UnitTests.IslandSurvivor.Logic;

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
