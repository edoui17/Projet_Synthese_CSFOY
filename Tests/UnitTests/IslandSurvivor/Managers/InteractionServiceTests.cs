using System.Collections.Generic;
using IslandSurvivor.Interfaces;
using IslandSurvivor.Managers;
using Xunit;
using Moq;

namespace Tests.UnitTests.IslandSurvivor.Managers;

public class InteractionServiceTests
{
    [Fact]
    public void GetBestInteractable_ShouldReturnClosest_WhenMultipleExist()
    {
        // Arrange
        var service = new InteractionService();
        var mock1 = new Mock<IInteractable>();
        mock1.Setup(m => m.IsInteractable).Returns(true);
        mock1.Setup(m => m.GetDistanceTo(0, 0)).Returns(10f);

        var mock2 = new Mock<IInteractable>();
        mock2.Setup(m => m.IsInteractable).Returns(true);
        mock2.Setup(m => m.GetDistanceTo(0, 0)).Returns(5f);

        var interactables = new List<IInteractable> { mock1.Object, mock2.Object };

        // Act
        var result = service.GetBestInteractable(0, 0, interactables);

        // Assert
        Assert.Equal(mock2.Object, result);
    }

    [Fact]
    public void GetBestInteractable_ShouldSkipNonInteractable()
    {
        // Arrange
        var service = new InteractionService();
        var mock1 = new Mock<IInteractable>();
        mock1.Setup(m => m.IsInteractable).Returns(false);
        mock1.Setup(m => m.GetDistanceTo(0, 0)).Returns(2f);

        var mock2 = new Mock<IInteractable>();
        mock2.Setup(m => m.IsInteractable).Returns(true);
        mock2.Setup(m => m.GetDistanceTo(0, 0)).Returns(10f);

        var interactables = new List<IInteractable> { mock1.Object, mock2.Object };

        // Act
        var result = service.GetBestInteractable(0, 0, interactables);

        // Assert
        Assert.Equal(mock2.Object, result);
    }

    [Fact]
    public void GetBestInteractable_ShouldReturnNull_WhenNoneAreInteractable()
    {
        // Arrange
        var service = new InteractionService();
        var mock1 = new Mock<IInteractable>();
        mock1.Setup(m => m.IsInteractable).Returns(false);

        var interactables = new List<IInteractable> { mock1.Object };

        // Act
        var result = service.GetBestInteractable(0, 0, interactables);

        // Assert
        Assert.Null(result);
    }
}
