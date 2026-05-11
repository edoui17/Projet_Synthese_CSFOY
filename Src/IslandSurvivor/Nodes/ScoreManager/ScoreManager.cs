using Godot;
using Core.Interfaces.Stats;
using Core.Events;
using IslandSurvivor.Resources.Stats;
using IslandSurvivor.Globals;

namespace IslandSurvivor.Nodes.StatsManager;

public partial class ScoreManager : Node
{
    private SessionResource? m_sessionResource;
    private IScoreTracker m_scoreTracker;
    private Core.Interfaces.IEventBus m_eventBus;

    [Export]
    public SessionResource? SessionResource
    {
        get => m_sessionResource;
        set => m_sessionResource = value;
    }

    [Signal]
    public delegate void ScoreChangedEventHandler(int p_previousScore, int p_newScore);

    public override void _Ready()
    {
        base._Ready();

        m_scoreTracker = ServiceRegistry.Instance.ScoreTracker;
        m_eventBus = ServiceRegistry.Instance.EventBus;

        if (m_sessionResource != null)
        {
            m_scoreTracker.Initialize(
                m_sessionResource.StartingScore,
                m_sessionResource.StartingMapCount,
                m_sessionResource.StartingCharacterId
            );
        }
        else
        {
            GD.PushWarning("ScoreManager: SessionResource is not assigned. Initializing with default values.");
            m_scoreTracker.Initialize(0, 0, string.Empty);
        }

        m_eventBus.Subscribe<ScoreChangedEvent>(OnCoreScoreChanged);
    }

    public int GetCurrentScore()
    {
        return m_scoreTracker.CurrentScore;
    }

    public int GetHighScore()
    {
        return m_scoreTracker.HighScore;
    }

    public void AddScore(int p_amount)
    {
        m_scoreTracker.AddScore(p_amount);
    }

    public void UpdateHighScore()
    {
        m_scoreTracker.UpdateHighScore();
    }

    public IScoreTracker GetTracker()
    {
        return m_scoreTracker;
    }

    private void OnCoreScoreChanged(ScoreChangedEvent p_event)
    {
        GD.Print($"[ScoreManager] Score updated visually (console): {p_event.PreviousScore} -> {p_event.NewScore}");
        EmitSignal(SignalName.ScoreChanged, p_event.PreviousScore, p_event.NewScore);
    }

    protected override void Dispose(bool p_disposing)
    {
        if (p_disposing && m_eventBus != null)
        {
            m_eventBus.Unsubscribe<ScoreChangedEvent>(OnCoreScoreChanged);
        }
        base.Dispose(p_disposing);
    }
}
