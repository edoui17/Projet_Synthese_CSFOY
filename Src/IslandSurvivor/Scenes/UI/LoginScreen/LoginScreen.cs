using Godot;
using Core.Utils;
using IslandSurvivor.Globals;
using IslandSurvivor.Utils;

namespace IslandSurvivor.Scenes.UI.LoginScreen;

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
        m_passwordField.FocusNeighborBottom = m_loginBtn.GetPath();
        m_loginBtn.FocusNeighborTop = m_passwordField.GetPath();
        m_loginBtn.FocusNeighborBottom = m_offlineBtn.GetPath();
        m_offlineBtn.FocusNeighborTop = m_loginBtn.GetPath();

        m_usernameField.GrabFocus();
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

        m_errorLabel.Visible = false;
        m_loginBtn.Disabled = true;
        m_offlineBtn.Disabled = true;

        GD.Print($"[LoginScreen] Attempting login for user: {username}");

        try
        {
            string? token = await ServiceRegistry.Instance.ApiService.LoginAsync(username, password);

            if (!string.IsNullOrEmpty(token))
            {
                GD.Print("[LoginScreen] Login successful. Storing token and redirecting.");
                SessionProvider.StoreToken(token);
                GetTree().ChangeSceneToFile("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
            }
            else
            {
                ShowError("Échec de la connexion. Vérifiez vos identifiants.");
                m_loginBtn.Disabled = false;
                m_offlineBtn.Disabled = false;
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[LoginScreen] Network error during login: {ex.Message}");
            ShowError("Erreur réseau. Serveur indisponible.");
            m_loginBtn.Disabled = false;
            m_offlineBtn.Disabled = false;
        }
    }

    private void OnOfflinePressed()
    {
        GD.Print("[LoginScreen] Offline mode requested.");
        EmitSignal(SignalName.OfflineModeRequested);
    }

    private void OnShowPasswordToggled(bool p_toggledOn)
    {
        m_passwordField.Secret = !p_toggledOn;
    }

    private void ShowError(string p_message)
    {
        m_errorLabel.Text = p_message;
        m_errorLabel.Visible = true;
    }
}
