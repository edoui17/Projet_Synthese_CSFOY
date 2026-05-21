using Godot;
using IslandSurvivor.Managers;

public partial class ErrorPopup : CanvasLayer
{
    private Button m_retryBtn = null!;
    private Button m_offlineBtn = null!;

    public override void _Ready()
    {
        m_retryBtn = GetNode<Button>("Panel/VBoxContainer/HBoxContainer/RetryBtn");
        m_offlineBtn = GetNode<Button>("Panel/VBoxContainer/HBoxContainer/OfflineBtn");

        m_retryBtn.Pressed += OnRetryPressed;
        m_offlineBtn.Pressed += OnOfflinePressed;

        m_retryBtn.GrabFocus();
    }

    private void OnRetryPressed()
    {
        GameManager.Instance.RetryInitialization();
        QueueFree();
    }

    private void OnOfflinePressed()
    {
        GameManager.Instance.SetGuestMode();
        QueueFree();
    }
}
