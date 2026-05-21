using Godot;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using IslandSurvivor.Globals;
using IslandSurvivor.Utils;

namespace IslandSurvivor.Managers;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    private AppStatus m_status = AppStatus.Loading;
    public AppStatus Status => m_status;

    private bool m_isGuest = false;
    public bool IsGuest => m_isGuest;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _Ready()
    {
        InitializeGameAsync();
    }

    private async void InitializeGameAsync()
    {
        m_status = AppStatus.Loading;
        GD.Print("[GameManager] Initializing game...");

        IApiService apiService = ServiceRegistry.Instance.ApiService;
        string? storedToken = SessionProvider.GetStoredToken();

        if (!string.IsNullOrEmpty(storedToken))
        {
            GD.Print("[GameManager] Found stored token, validating...");
            apiService.SetSessionToken(storedToken);

            try
            {
                PlayerProfile? profile = await apiService.GetProfileAsync();
                if (profile != null)
                {
                    GD.Print("[GameManager] Session validated successfully.");
                    m_status = AppStatus.Ready;
                    GetTree().ChangeSceneToFile("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
                    return;
                }
                else
                {
                    GD.Print("[GameManager] Stored token is invalid or expired.");
                    SessionProvider.ClearToken();
                    apiService.SetSessionToken(null);
                }
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"[GameManager] Network error during validation: {ex.Message}");
                m_status = AppStatus.Error;
                ShowErrorPopup();
                return;
            }
        }

        GD.Print("[GameManager] No valid session. Redirecting to Login.");
        m_status = AppStatus.Ready;
        GetTree().ChangeSceneToFile("res://Scenes/Login/Login.tscn");
    }

    public void SetGuestMode()
    {
        GD.Print("[GameManager] Entering Guest Mode.");
        m_isGuest = true;
        m_status = AppStatus.Ready;
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
    }

    public void RetryInitialization()
    {
        InitializeGameAsync();
    }

    private void ShowErrorPopup()
    {
        GD.PrintErr("[GameManager] ERROR: API inaccessible.");
        var popupScene = GD.Load<PackedScene>("res://Scenes/UI/ErrorPopup/ErrorPopup.tscn");
        var popup = popupScene.Instantiate();
        AddChild(popup);
    }
}
