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

    public NavigationServiceTests()
    {
        m_signalManagerMock = new Mock<ISignalManager>();
        m_eventBusMock = new Mock<IEventBus>();
        m_navigationService = new NavigationService(m_signalManagerMock.Object, m_eventBusMock.Object);
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

        bool result = m_navigationService.TryNavigate(destination);

        Assert.True(result);
        m_eventBusMock.Verify(b => b.Publish(It.Is<NavigationRequestedEvent>(e => e.Destination == destination)), Times.Once);
    }
}
