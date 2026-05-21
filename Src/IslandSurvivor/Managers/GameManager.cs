using Godot;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Core.Events;
using Core.Utils;
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
                ProfileResponse? profileResponse = await apiService.GetProfileAsync();
                if (profileResponse != null)
                {
                    GD.Print("[GameManager] Session validated successfully. Mapping profile...");
                    PlayerProfile profile = ProfileMapper.MapToDomain(profileResponse);

                    GD.Print("[GameManager] Publishing ProfileLoadedEvent.");
                    ServiceRegistry.Instance.EventBus.Publish(new ProfileLoadedEvent(profile));

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

        IApiService apiService = ServiceRegistry.Instance.ApiService;
        ProfileResponse? cachedProfile = apiService.GetCachedProfile();

        if (cachedProfile != null)
        {
            GD.Print("[GameManager] Found cached profile for Guest mode. Mapping...");
            PlayerProfile profile = ProfileMapper.MapToDomain(cachedProfile);
            ServiceRegistry.Instance.EventBus.Publish(new ProfileLoadedEvent(profile));
        }
        else
        {
            GD.Print("[GameManager] No cached profile found. Initializing default guest profile.");
            PlayerProfile defaultProfile = new PlayerProfile
            {
                Player = new Core.Domain.Player { Username = "Guest" }
            };
            ServiceRegistry.Instance.EventBus.Publish(new ProfileLoadedEvent(defaultProfile));
        }

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
