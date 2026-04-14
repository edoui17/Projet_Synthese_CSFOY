using Godot;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using IslandSurvivor.Resources.Stats;
using IslandSurvivor.Globals;

namespace IslandSurvivor.Nodes.StatsManager;

public partial class ScoreManager : Node
{
    private SessionResource? m_sessionResource;
    private IScoreTracker m_scoreTracker;

    [Export]
    public SessionResource? SessionResource
    {
        get => m_sessionResource;
        set => m_sessionResource = value;
    }

    [Signal]
    public delegate void ScoreChangedEventHandler(int p_previousScore, int p_newScore);

    public ScoreManager()
    {
        // Inject GodotSaveService into ScoreTracker
        m_scoreTracker = new ScoreTracker(new GodotSaveService());
    }

    public override void _Ready()
    {
        base._Ready();

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

        m_scoreTracker.OnScoreChanged.AddListener(OnCoreScoreChanged);
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

    private void OnCoreScoreChanged(object? p_sender, IScoreTracker.ScoreChangedEventArgs p_args)
    {
        EmitSignal(SignalName.ScoreChanged, p_args.PreviousScore, p_args.NewScore);
    }

    protected override void Dispose(bool p_disposing)
    {
        if (p_disposing && m_scoreTracker != null)
        {
            m_scoreTracker.OnScoreChanged.RemoveListener(OnCoreScoreChanged);
        }
        base.Dispose(p_disposing);
    }
}
