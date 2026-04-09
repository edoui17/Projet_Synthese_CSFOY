namespace Tests.UnitTests.Core.Navigation;

using System.Linq;
using Moq;
using Xunit;
using global::Core.Domain;
using global::Core.Domain.Models;
using global::Core.Interfaces;
using global::Core.Interfaces.Navigation;
using global::Core.Managers.Navigation;

public class NavigationServiceTests
{
    private readonly Mock<ISignalManager> m_signalManagerMock;
    private readonly NavigationService m_navigationService;
    private readonly Mock<IInventoryManager> m_inventoryManagerMock;

    public NavigationServiceTests()
    {
        m_signalManagerMock = new Mock<ISignalManager>();
        m_navigationService = new NavigationService(m_signalManagerMock.Object);
        m_inventoryManagerMock = new Mock<IInventoryManager>();
    }

    [Fact]
    public void GenerateDestinations_ShouldReturnCorrectCount()
    {
        int count = 3;
        var destinations = m_navigationService.GenerateDestinations(count);

        Assert.Equal(count, destinations.Count);
        foreach (var dest in destinations)
        {
            Assert.False(string.IsNullOrEmpty(dest.Id));
            Assert.False(string.IsNullOrEmpty(dest.Biome));
            Assert.True(dest.Difficulty > 0);
            Assert.True(dest.ResourceCost >= 0); // The free island has 0 cost
        }
    }

    [Fact]
    public void TryNavigate_WithHomeIsland_ShouldAlwaysSucceed()
    {
        bool result = m_navigationService.TryNavigate(m_inventoryManagerMock.Object, IslandDestination.HomeIsland);

        Assert.True(result);
        m_signalManagerMock.Verify(s => s.EmitNavigationRequested(m_navigationService, IslandDestination.HomeIsland), Times.Once);
    }

    [Fact]
    public void TryNavigate_WithInsufficientResources_ShouldFail()
    {
        var destination = new IslandDestination("test_id", "test_path", "Normal", 5, 5, 5);

        // Inventory has no resources
        m_inventoryManagerMock.Setup(i => i.GetMaterialCount(It.IsAny<string>())).Returns(0);

        bool result = m_navigationService.TryNavigate(m_inventoryManagerMock.Object, destination);

        Assert.False(result);
        m_signalManagerMock.Verify(s => s.EmitNavigationRequested(It.IsAny<object>(), It.IsAny<IslandDestination>()), Times.Never);
    }

    [Fact]
    public void TryNavigate_WithSufficientResources_ShouldDeductAndSucceed()
    {
        var destination = new IslandDestination("test_id", "test_path", "Normal", 5, 5, 5);

        m_inventoryManagerMock.Setup(i => i.GetMaterialCount(It.IsAny<string>())).Returns(10); // Plenty of resources

        bool result = m_navigationService.TryNavigate(m_inventoryManagerMock.Object, destination);

        Assert.True(result);

        // Ensure all 4 basic resources are deducted
        string[] resources = { "Viande", "Bois", "Roche", "Or" };
        foreach (var res in resources)
        {
            m_inventoryManagerMock.Verify(i => i.RemoveMaterial(res, 5), Times.Once);
            m_signalManagerMock.Verify(s => s.EmitResourceSpent(m_navigationService, res, 5), Times.Once);
        }

        // We decoupled emitting the NavigationRequested to wait for Portal interaction, so it should not be emitted here anymore.
        m_signalManagerMock.Verify(s => s.EmitNavigationRequested(It.IsAny<object>(), It.IsAny<IslandDestination>()), Times.Never);
    }
}
