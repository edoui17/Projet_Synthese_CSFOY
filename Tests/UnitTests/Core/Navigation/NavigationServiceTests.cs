namespace Tests.UnitTests.Core.Navigation;

using System.Linq;
using Moq;
using Xunit;
using global::Core.Domain;
using global::Core.Domain.Models;
using global::Core.Interfaces;
using global::Core.Interfaces.Navigation;
using global::Core.Managers.Navigation;
using global::Core.Events;

public class NavigationServiceTests
{
    private readonly Mock<ISignalManager> m_signalManagerMock;
    private readonly NavigationService m_navigationService;
    private readonly Mock<IEventBus> m_eventBusMock;
    private readonly Mock<IShopManager> m_shopManagerMock;
    private readonly Mock<IInventoryManager> m_inventoryManagerMock;

    public NavigationServiceTests()
    {
        m_signalManagerMock = new Mock<ISignalManager>();
        m_eventBusMock = new Mock<IEventBus>();
        m_shopManagerMock = new Mock<IShopManager>();
        m_inventoryManagerMock = new Mock<IInventoryManager>();
        m_navigationService = new NavigationService(m_signalManagerMock.Object, m_eventBusMock.Object, m_shopManagerMock.Object, m_inventoryManagerMock.Object);
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
    public void TryNavigate_ShouldPublishEventAndReturnTrue()
    {
        var destination = new IslandDestination("test_id", "test_path", "Normal", 5, 5, 5);
        m_shopManagerMock.Setup(s => s.CanAffordIsland(It.IsAny<IInventoryManager>(), It.IsAny<int>())).Returns(true);

        bool result = m_navigationService.TryNavigate(destination);

        Assert.True(result);
        m_eventBusMock.Verify(b => b.Publish(It.Is<NavigationRequestedEvent>(e => e.Destination == destination)), Times.Once);
    }

    [Fact]
    public void TryNavigate_WhenCannotAfford_ShouldReturnFalseAndNotPublishEvent()
    {
        var destination = new IslandDestination("test_id", "test_path", "Normal", 5, 5, 5);
        m_shopManagerMock.Setup(s => s.CanAffordIsland(It.IsAny<IInventoryManager>(), It.IsAny<int>())).Returns(false);

        bool result = m_navigationService.TryNavigate(destination);

        Assert.False(result);
        m_eventBusMock.Verify(b => b.Publish(It.IsAny<NavigationRequestedEvent>()), Times.Never);
    }
}
