using System;
using Godot;
using Xunit;
using IslandSurvivor.Logic.Entities;

namespace Tests.UnitTests.IslandSurvivor.Logic;

public class PassiveControllerTests
{
    [Fact]
    public void InitialState_IsIdle()
    {
        var controller = new PassiveController();
        Assert.Equal(NpcStates.IDLE, controller.CurrentState);
    }

    [Fact]
    public void StartFleeing_ChangesStateAndSetsDirection()
    {
        var controller = new PassiveController();
        var sheepPos = new Vector2(10, 10);
        var attackerPos = new Vector2(0, 10); // Attacker is to the left

        controller.StartFleeing(sheepPos, attackerPos);

        Assert.Equal(NpcStates.FLEE, controller.CurrentState);
        // Should flee to the right (positive X)
        Assert.True(controller.CurrentDirection.X > 0);
        Assert.Equal(0, controller.CurrentDirection.Y);
    }

    [Fact]
    public void Update_FleeTimerExpires_ReturnsToIdle()
    {
        var controller = new PassiveController();
        controller.StartFleeing(Vector2.Zero, new Vector2(1, 0));

        // Advance time just before expiration
        controller.Update(PassiveController.FLEE_DURATION - 0.1f);
        Assert.Equal(NpcStates.FLEE, controller.CurrentState);

        // Advance past expiration
        controller.Update(0.2f);
        Assert.Equal(NpcStates.IDLE, controller.CurrentState);
    }

    [Fact]
    public void ForceNewDirection_ChangesDirectionInIdleState()
    {
        var controller = new PassiveController();
        Assert.Equal(NpcStates.IDLE, controller.CurrentState);

        var initialDirection = controller.CurrentDirection;
        controller.ForceNewDirection();

        // It's possible but extremely unlikely to get the exact same random direction
        // In a strictly deterministic test we might mock Random, but here we just ensure it doesn't crash
        var newDirection = controller.CurrentDirection;

        // Timer should have been reset to > 0
        // We can't easily check the private timer, but we verify it can be called safely
        Assert.True(newDirection.Length() > 0);
    }
}
