using Godot;

namespace IslandSurvivor.Nodes.StatsManager;

public partial class ScoreUI : Label
{
  [Export]
  private ScoreManager m_scoreManager;

  public override void _Ready()
  {
    if (m_scoreManager != null)
    {
      m_scoreManager.ScoreChanged += OnScoreChanged;
    }
    else
    {
      GD.PushWarning("ScoreUI: ScoreManager reference is missing.");
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
