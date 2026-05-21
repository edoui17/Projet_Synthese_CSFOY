using Godot;
using IslandSurvivor.Globals;
using IslandSurvivor.Utils;

public partial class Login : Control
{
    private LineEdit m_usernameField = null!;
    private LineEdit m_passwordField = null!;
    private Button m_loginBtn = null!;
    private Label m_statusLabel = null!;

    public override void _Ready()
    {
        m_usernameField = GetNode<LineEdit>("Panel/VBoxContainer/UsernameField");
        m_passwordField = GetNode<LineEdit>("Panel/VBoxContainer/PasswordField");
        m_loginBtn = GetNode<Button>("Panel/VBoxContainer/LoginBtn");
        m_statusLabel = GetNode<Label>("Panel/VBoxContainer/StatusLabel");

        m_loginBtn.Pressed += OnLoginPressed;

        m_usernameField.GrabFocus();
    }

    private async void OnLoginPressed()
    {
        string username = m_usernameField.Text.Trim();
        string password = m_passwordField.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            m_statusLabel.Text = "Please fill in all fields.";
            return;
        }

        m_statusLabel.Text = "Connecting...";
        m_loginBtn.Disabled = true;

        string? token = await ServiceRegistry.Instance.ApiService.LoginAsync(username, password);

        if (!string.IsNullOrEmpty(token))
        {
            SessionProvider.StoreToken(token);
            GetTree().ChangeSceneToFile("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
        }
        else
        {
            m_statusLabel.Text = "Login failed. Check your credentials.";
            m_loginBtn.Disabled = false;
        }
    }
}
