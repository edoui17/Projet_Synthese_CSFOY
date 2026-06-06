using Godot;
using Core.Utils;
using IslandSurvivor.Globals;
using IslandSurvivor.Utils;

namespace IslandSurvivor.Scenes.UI;

public partial class LoginScreen : Control
{
    [Signal]
    public delegate void OfflineModeRequestedEventHandler();

    private LineEdit m_usernameField = null!;
    private LineEdit m_passwordField = null!;
    private Button m_loginBtn = null!;
    private Button m_offlineBtn = null!;
    private TextureButton m_showPasswordBtn = null!;
    private Label m_errorLabel = null!;

    public override void _Ready()
    {
        // Bind nodes using Unique Names
        m_usernameField = GetNode<LineEdit>("%UsernameField");
        m_passwordField = GetNode<LineEdit>("%PasswordField");
        m_loginBtn = GetNode<Button>("%LoginBtn");
        m_offlineBtn = GetNode<Button>("%OfflineBtn");
        m_showPasswordBtn = GetNode<TextureButton>("%ShowPasswordBtn");
        m_errorLabel = GetNode<Label>("%ErrorLabel");

        // Signals
        m_loginBtn.Pressed += OnLoginPressed;
        m_offlineBtn.Pressed += OnOfflinePressed;
        m_showPasswordBtn.Toggled += OnShowPasswordToggled;

        m_usernameField.TextSubmitted += _ => OnLoginPressed();
        m_passwordField.TextSubmitted += _ => OnLoginPressed();

        // Focus handling for TAB navigation
        m_usernameField.FocusNeighborBottom = m_passwordField.GetPath();

        m_passwordField.FocusNeighborTop = m_usernameField.GetPath();
        m_passwordField.FocusNeighborBottom = m_showPasswordBtn.GetPath();

        m_showPasswordBtn.FocusNeighborTop = m_passwordField.GetPath();
        m_showPasswordBtn.FocusNeighborBottom = m_loginBtn.GetPath();
        m_showPasswordBtn.TooltipText = "Afficher/Masquer le mot de passe";

        m_loginBtn.FocusNeighborTop = m_showPasswordBtn.GetPath();
        m_loginBtn.FocusNeighborBottom = m_offlineBtn.GetPath();

        m_offlineBtn.FocusNeighborTop = m_loginBtn.GetPath();
        m_offlineBtn.FocusNeighborBottom = m_usernameField.GetPath();
        m_usernameField.FocusNeighborTop = m_offlineBtn.GetPath();

        SetupButtonJuice(m_loginBtn);
        SetupButtonJuice(m_offlineBtn);
        SetupButtonJuice(m_showPasswordBtn);

        m_usernameField.GrabFocus();
    }

    private void SetupButtonJuice(Control p_btn)
    {
        p_btn.FocusMode = FocusModeEnum.All;
        p_btn.MouseDefaultCursorShape = CursorShape.PointingHand;

        p_btn.MouseEntered += () => p_btn.GrabFocus();

        p_btn.FocusEntered += () =>
        {
            if (p_btn.HasMeta("tween"))
            {
                p_btn.GetMeta("tween").As<Tween>()?.Kill();
            }
            Tween tween = CreateTween();
            p_btn.SetMeta("tween", tween);
            p_btn.PivotOffset = p_btn.Size / 2f;
            tween.TweenProperty(p_btn, "scale", new Vector2(1.05f, 1.05f), 0.1f);
        };

        p_btn.FocusExited += () =>
        {
            if (p_btn.HasMeta("tween"))
            {
                p_btn.GetMeta("tween").As<Tween>()?.Kill();
            }
            Tween tween = CreateTween();
            p_btn.SetMeta("tween", tween);
            p_btn.PivotOffset = p_btn.Size / 2f;
            tween.TweenProperty(p_btn, "scale", new Vector2(1.0f, 1.0f), 0.1f);
        };
    }

    private async void OnLoginPressed()
    {
        string username = m_usernameField.Text.Trim();
        string password = m_passwordField.Text;

        // Local Validation using Core Logic
        var (isValid, errorMessage) = LoginValidator.Validate(username, password);

        if (!isValid)
        {
            ShowError(errorMessage ?? "Erreur de validation.");
            return;
        }

        // UX: Show loading state and lock inputs
        m_errorLabel.Text = "Connexion en cours...";
        m_errorLabel.Modulate = new Color(1, 1, 1, 1); // Neutral white
        m_errorLabel.Visible = true;

        SetInputsEnabled(false);

        GD.Print($"[LoginScreen] Attempting login for user: {username}");

        try
        {
            string? token = await ServiceRegistry.Instance.ApiService.LoginAsync(username, password);

            if (!string.IsNullOrEmpty(token))
            {
                GD.Print("[LoginScreen] Login successful. Storing token and triggering initialization flow.");
                SessionProvider.StoreToken(token);

                // Trigger full initialization flow in GameManager to handle LoadingScreen and Sync
                IslandSurvivor.Managers.GameManager.Instance.InitializeGameAsync();
            }
            else
            {
                ShowError("Identifiants invalides.");
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[LoginScreen] Network error during login: {ex.Message}");

            // Check for Server/Network issues (not 401)
            ShowError("Serveur API indisponible.");
        }
        finally
        {
            // Re-enable inputs only if we haven't changed scene (if login failed)
            if (IsInsideTree())
            {
                SetInputsEnabled(true);
            }
        }
    }

    private void SetInputsEnabled(bool p_enabled)
    {
        m_usernameField.Editable = p_enabled;
        m_passwordField.Editable = p_enabled;
        m_loginBtn.Disabled = !p_enabled;
        m_offlineBtn.Disabled = !p_enabled;
    }

    private void OnOfflinePressed()
    {
        GD.Print("[LoginScreen] Offline mode requested.");
        SignalManager.Instance.EmitOfflineModeRequested();
    }

    private void OnShowPasswordToggled(bool p_toggledOn)
    {
        m_passwordField.Secret = !p_toggledOn;
    }

    private void ShowError(string p_message)
    {
        m_errorLabel.Text = p_message;
        m_errorLabel.Modulate = new Color(1, 0.333f, 0.333f, 1); // Red #ff5555
        m_errorLabel.Visible = true;
    }
}
