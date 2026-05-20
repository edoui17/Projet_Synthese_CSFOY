using Godot;

namespace IslandSurvivor.Nodes.StatsManager;

public partial class ScoreUI : Label
{
    [Export]
    private ScoreManager m_scoreManager = null!;

    public override void _Ready()
    {
        if (m_scoreManager == null)
        {
            // Try to find it dynamically globally
            m_scoreManager = GetTree().Root.GetNodeOrNull<ScoreManager>("Main/ScoreManager");
        }

        if (m_scoreManager == null)
        {
            // Alternative search using groups if it's there
            var currentScene = GetTree().CurrentScene;
            if (currentScene != null)
            {
                m_scoreManager = currentScene.GetNodeOrNull<ScoreManager>("ScoreManager");
            }
        }

        if (m_scoreManager != null)
        {
            m_scoreManager.ScoreChanged += OnScoreChanged;

            // Set initial score if possible
            Text = $"Score {m_scoreManager.GetCurrentScore()}";
        }
        else
        {
            GD.PushWarning("ScoreUI: ScoreManager reference is missing and could not be resolved.");
        }
    }

    private void OnScoreChanged(int p_previousScore, int p_newScore)
    {
        Text = $"Score {p_newScore}";
        GD.Print($"[ScoreUI] Score updated! Previous: {p_previousScore}, New: {p_newScore}");
    }

    public override void _ExitTree()
    {
        if (m_scoreManager != null)
        {
            m_scoreManager.ScoreChanged -= OnScoreChanged;
        }
    }
}
