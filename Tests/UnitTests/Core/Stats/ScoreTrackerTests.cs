using Xunit;
using Core.Managers.Stats;
using Core.Interfaces;
using Core.Interfaces.Stats;
using Core.Events;
using Moq;

namespace Tests.UnitTests.Core.Stats;

public class ScoreTrackerTests
{
    private Mock<ISaveService> m_mockSaveService;
    private Mock<IEventBus> m_mockEventBus;
    private ScoreTracker m_scoreTracker;

    public ScoreTrackerTests()
    {
        m_mockSaveService = new Mock<ISaveService>();
        m_mockEventBus = new Mock<IEventBus>();
        // Return empty string by default to simulate no high score saved yet
        m_mockSaveService.Setup(s => s.LoadData(It.IsAny<string>())).Returns("");
        m_scoreTracker = new ScoreTracker(m_mockSaveService.Object, m_mockEventBus.Object);
        m_scoreTracker.Initialize(0, 0, "test_char");
    }

    [Fact]
    public void AddScore_WithPositiveAmount_IncreasesScore()
    {
        // Act
        m_scoreTracker.AddScore(50);

        // Assert
        Assert.Equal(50, m_scoreTracker.CurrentScore);
    }

    [Fact]
    public void AddScore_WithNegativeAmount_DoesNotChangeScore()
    {
        // Arrange
        m_scoreTracker.AddScore(50);

        // Act
        m_scoreTracker.AddScore(-10);

        // Assert
        Assert.Equal(50, m_scoreTracker.CurrentScore);
    }

    [Fact]
    public void AddScore_WithPositiveAmount_PublishesEvent()
    {
        // Act
        m_scoreTracker.AddScore(100);

        // Assert
        m_mockEventBus.Verify(eb => eb.Publish(It.Is<ScoreChangedEvent>(e => e.PreviousScore == 0 && e.NewScore == 100)), Times.Once);
    }

    [Fact]
    public void UpdateHighScore_WhenCurrentScoreIsHigher_UpdatesAndSavesHighScore()
    {
        // Arrange
        m_scoreTracker.AddScore(150);

        // Act
        m_scoreTracker.UpdateHighScore();

        // Assert
        Assert.Equal(150, m_scoreTracker.HighScore);
        m_mockSaveService.Verify(s => s.SaveData("highscore.json", It.Is<string>(j => j.Contains("150"))), Times.Once);
    }

    [Fact]
    public void UpdateHighScore_WhenCurrentScoreIsLower_DoesNotUpdateHighScore()
    {
        // Arrange
        // Simulate an existing high score
        m_mockSaveService.Setup(s => s.LoadData("highscore.json")).Returns("{\"HighScore\":200}");
        var scoreTrackerWithHighScore = new ScoreTracker(m_mockSaveService.Object, m_mockEventBus.Object);
        scoreTrackerWithHighScore.Initialize(0, 0, "test_char");

        scoreTrackerWithHighScore.AddScore(50);

        // Act
        scoreTrackerWithHighScore.UpdateHighScore();

        // Assert
        Assert.Equal(200, scoreTrackerWithHighScore.HighScore); // Should remain 200
        m_mockSaveService.Verify(s => s.SaveData("highscore.json", It.IsAny<string>()), Times.Never);
    }
}
