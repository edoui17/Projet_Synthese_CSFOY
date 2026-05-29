using Godot;

namespace IslandSurvivor.Nodes;

public partial class ScoreUI : Label
{
    [Export]
    private ScoreManager m_scoreManager = null!;

    private Tween m_currentTween = null!;

    public override void _Ready()
    {
        if (m_scoreManager == null)
        {
            // Try to find it dynamically globally
            m_scoreManager = (GetTree().GetFirstNodeInGroup("ScoreManager") as ScoreManager)!;
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

        if (p_previousScore != p_newScore && IsInsideTree())
        {
            PivotOffset = Size / 2f;

            m_currentTween?.Kill();
            m_currentTween = CreateTween();

            Color highlightColor = p_newScore > p_previousScore ? Colors.LimeGreen : Colors.IndianRed;

            m_currentTween.TweenProperty(this, "scale", new Vector2(1.2f, 1.2f), 0.1f);
            m_currentTween.Parallel().TweenProperty(this, "modulate", highlightColor, 0.1f);
            m_currentTween.TweenProperty(this, "scale", Vector2.One, 0.2f);
            m_currentTween.Parallel().TweenProperty(this, "modulate", Colors.White, 0.2f);
        }
    }

    public override void _ExitTree()
    {
        if (m_scoreManager != null)
        {
            m_scoreManager.ScoreChanged -= OnScoreChanged;
        }
    }
}
