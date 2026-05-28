using Godot;
using System;
using Core.Events;
using Core.Interfaces.Stats;
using Core.Managers.Stats;
using IslandSurvivor.Globals;

public partial class VictoryMenu : CanvasLayer
{
    private Label m_scoreLabel;
    private Label m_highScoreLabel;
    private Label m_mapsClearedLabel;
    private Label m_playerStatsLabel;
    private Action<BossDiedEvent> m_onBossDiedDelegate;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        m_scoreLabel = GetNode<Label>("PanelContainer/VBoxContainer/ScoreLabel");
        m_highScoreLabel = GetNode<Label>("PanelContainer/VBoxContainer/HighScoreLabel");
        m_mapsClearedLabel = GetNode<Label>("PanelContainer/VBoxContainer/MapsClearedLabel");
        m_playerStatsLabel = GetNode<Label>("PanelContainer/VBoxContainer/PlayerStatsLabel");

        m_onBossDiedDelegate = OnBossDied;

        if (ServiceRegistry.Instance != null && ServiceRegistry.Instance.EventBus != null)
        {
            ServiceRegistry.Instance.EventBus.Subscribe<BossDiedEvent>(m_onBossDiedDelegate);
        }
    }

    private async void OnBossDied(BossDiedEvent p_event)
    {
        // Add a slight delay for dramatic effect
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        UpdateStats();

        Visible = true;
        GetTree().Paused = true;

        var returnBtn = GetNodeOrNull<TextureButton>("PanelContainer/VBoxContainer/ReturnMenuBtn");
        if (returnBtn != null)
        {
            returnBtn.GrabFocus();
        }
    }

    private void UpdateStats()
    {
        if (ServiceRegistry.Instance == null) return;

        IScoreTracker scoreTracker = ServiceRegistry.Instance.ScoreTracker;
        IStatTracker statTracker = ServiceRegistry.Instance.StatTracker;

        if (scoreTracker != null)
        {
            scoreTracker.UpdateHighScore();
            m_scoreLabel.Text = $"Score: {scoreTracker.CurrentScore}";
            m_highScoreLabel.Text = $"High Score: {scoreTracker.HighScore}";
            m_mapsClearedLabel.Text = $"Maps Cleared: {scoreTracker.MapCount}";
        }

        if (statTracker != null)
        {
            float level = statTracker.GetCurrentValue(StatType.Level);
            float attack = statTracker.GetCurrentValue(StatType.Attack);
            float speed = statTracker.GetCurrentValue(StatType.Speed);
            float luck = statTracker.GetCurrentValue(StatType.Luck);
            float health = statTracker.GetEffectiveMaxValue(StatType.Health);

            m_playerStatsLabel.Text = $"Stats:\nLevel {level}\nMax Health: {health}\nAttack: {attack}\nSpeed: {speed}\nLuck: {luck}";
        }
    }

    public void _on_return_menu_btn_pressed()
    {
        GetTree().Paused = false;

        // Notify the SessionManager to clean up state before transitioning
        SignalManager.Instance?.EmitSignal(SignalManager.SignalName.SessionEnded, false);

        GetTree().ChangeSceneToFile("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
    }

    public override void _ExitTree()
    {
        if (ServiceRegistry.Instance != null && ServiceRegistry.Instance.EventBus != null && m_onBossDiedDelegate != null)
        {
            ServiceRegistry.Instance.EventBus.Unsubscribe<BossDiedEvent>(m_onBossDiedDelegate);
        }
    }
}
