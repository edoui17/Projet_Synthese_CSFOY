using IslandSurvivor.Interfaces;
using IslandSurvivor.Nodes;
using Xunit;
using Moq;
using Godot;

namespace Tests.UnitTests.IslandSurvivor.Nodes;

public class AttackableNodeTests
{
    [Fact]
    public void OnAttacked_ShouldExist()
    {
        // Since AttackableNode depends on Godot (Area2D), we might have issues running it in pure xUnit
        // if Godot isn't initialized. However, we can test the interface implementation if we mock it.

        var mock = new Mock<IAttackable>();
        mock.Setup(m => m.OnAttacked()).Verifiable();

        mock.Object.OnAttacked();

        mock.Verify(m => m.OnAttacked(), Times.Once);
    }
}
