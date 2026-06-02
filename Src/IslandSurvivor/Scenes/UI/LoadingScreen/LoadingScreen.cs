using Godot;

namespace IslandSurvivor.Scenes.UI;

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
    private int m_cooldownRemaining = 0;
    private string m_originalRetryText = string.Empty;

    public override void _Ready()
    {
        m_loadingSection = GetNode<Control>("Overlay/CenterContainer/LoadingSection");
        m_statusLabel = GetNode<Label>("Overlay/CenterContainer/LoadingSection/VBoxContainer/StatusLabel");
        m_spinner = GetNode<TextureRect>("Overlay/CenterContainer/LoadingSection/VBoxContainer/SpinnerContainer/Spinner");

        m_errorSection = GetNode<Control>("Overlay/CenterContainer/ErrorSection");
        m_errorMessageLabel = GetNode<Label>("Overlay/CenterContainer/ErrorSection/VBoxContainer/Message");
        m_retryBtn = GetNode<Button>("Overlay/CenterContainer/ErrorSection/VBoxContainer/HBoxContainer/RetryBtn");
        m_offlineBtn = GetNode<Button>("Overlay/CenterContainer/ErrorSection/VBoxContainer/HBoxContainer/OfflineBtn");

        m_originalRetryText = m_retryBtn.Text;
        m_retryBtn.Pressed += OnRetryPressed;
        m_offlineBtn.Pressed += OnOfflinePressed;

        SetupButton(m_retryBtn);
        SetupButton(m_offlineBtn);

        m_retryBtn.FocusNeighborRight = m_offlineBtn.GetPath();
        m_offlineBtn.FocusNeighborLeft = m_retryBtn.GetPath();

        ShowLoading();
    }

    public override void _Process(double p_delta)
    {
        if (m_loadingSection.Visible)
        {
            m_spinner.RotationDegrees += m_rotationSpeed * (float)p_delta;
        }
    }

    public void StartRetryCooldown(int p_seconds)
    {
        m_cooldownRemaining = p_seconds;
        m_retryBtn.Disabled = true;
        UpdateRetryButtonText();

        Timer timer = new Timer();
        timer.WaitTime = 1.0f;
        timer.OneShot = false;
        timer.Timeout += OnCooldownTick;
        AddChild(timer);
        timer.Start();
    }

    private void OnCooldownTick()
    {
        m_cooldownRemaining--;
        if (m_cooldownRemaining <= 0)
        {
            m_retryBtn.Disabled = false;
            m_retryBtn.Text = m_originalRetryText;

            // Cleanup timer
            foreach (Node child in GetChildren())
            {
                if (child is Timer t && t.IsConnected(Timer.SignalName.Timeout, Callable.From(OnCooldownTick)))
                {
                    t.Stop();
                    t.QueueFree();
                }
            }
            return;
        }

        UpdateRetryButtonText();
    }

    private void UpdateRetryButtonText()
    {
        m_retryBtn.Text = $"{m_originalRetryText} ({m_cooldownRemaining}s)";
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

    private void SetupButton(Button p_btn)
    {
        p_btn.FocusMode = Control.FocusModeEnum.All;
        p_btn.MouseDefaultCursorShape = Control.CursorShape.PointingHand;
        p_btn.PivotOffset = p_btn.Size / 2f;

        Tween? currentTween = null;

        p_btn.MouseEntered += () => p_btn.GrabFocus();

        p_btn.FocusEntered += () =>
        {
            p_btn.PivotOffset = p_btn.Size / 2f;
            currentTween?.Kill();
            currentTween = CreateTween();
            currentTween.TweenProperty(p_btn, "scale", new Vector2(1.05f, 1.05f), 0.1f);
        };

        p_btn.FocusExited += () =>
        {
            currentTween?.Kill();
            currentTween = CreateTween();
            currentTween.TweenProperty(p_btn, "scale", new Vector2(1.0f, 1.0f), 0.1f);
        };
    }
}
