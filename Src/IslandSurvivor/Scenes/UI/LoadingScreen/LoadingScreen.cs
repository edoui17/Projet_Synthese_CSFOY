using Godot;

namespace IslandSurvivor.Scenes.UI.LoadingScreen;

public partial class LoadingScreen : CanvasLayer
{
    [Signal]
    public delegate void RetryRequestedEventHandler();

    [Signal]
    public delegate void OfflineModeRequestedEventHandler();

    private Control m_loadingSection = null!;
    private Label m_statusLabel = null!;
    private TextureRect m_spinner = null!;

    private Control m_errorSection = null!;
    private Label m_errorMessageLabel = null!;
    private Button m_retryBtn = null!;
    private Button m_offlineBtn = null!;

    private float m_rotationSpeed = 360.0f;

    public override void _Ready()
    {
        m_loadingSection = GetNode<Control>("Overlay/CenterContainer/LoadingSection");
        m_statusLabel = GetNode<Label>("Overlay/CenterContainer/LoadingSection/VBoxContainer/StatusLabel");
        m_spinner = GetNode<TextureRect>("Overlay/CenterContainer/LoadingSection/VBoxContainer/SpinnerContainer/Spinner");

        m_errorSection = GetNode<Control>("Overlay/CenterContainer/ErrorSection");
        m_errorMessageLabel = GetNode<Label>("Overlay/CenterContainer/ErrorSection/VBoxContainer/Message");
        m_retryBtn = GetNode<Button>("Overlay/CenterContainer/ErrorSection/VBoxContainer/HBoxContainer/RetryBtn");
        m_offlineBtn = GetNode<Button>("Overlay/CenterContainer/ErrorSection/VBoxContainer/HBoxContainer/OfflineBtn");

        m_retryBtn.Pressed += OnRetryPressed;
        m_offlineBtn.Pressed += OnOfflinePressed;

        ShowLoading();
    }

    public override void _Process(double p_delta)
    {
        if (m_loadingSection.Visible)
        {
            m_spinner.RotationDegrees += m_rotationSpeed * (float)p_delta;
        }
    }

    public void ShowLoading(string p_message = "Synchronisation en cours...")
    {
        m_loadingSection.Visible = true;
        m_errorSection.Visible = false;
        m_statusLabel.Text = p_message;
    }

    public void ShowError(string p_message)
    {
        m_loadingSection.Visible = false;
        m_errorSection.Visible = true;
        m_errorMessageLabel.Text = p_message;
        m_retryBtn.GrabFocus();
    }

    private void OnRetryPressed()
    {
        EmitSignal(SignalName.RetryRequested);
    }

    private void OnOfflinePressed()
    {
        EmitSignal(SignalName.OfflineModeRequested);
    }
}
