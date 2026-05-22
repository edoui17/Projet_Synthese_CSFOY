using Godot;
using System.Threading.Tasks;
using Core.Domain;
using Core.Interfaces;
using Core.Events;
using Core.Utils;
using IslandSurvivor.Globals;
using IslandSurvivor.Utils;
using IslandSurvivor.Scenes.UI.LoadingScreen;

namespace IslandSurvivor.Managers;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    private AppStatus m_status = AppStatus.Loading;
    public AppStatus Status => m_status;

    private bool m_isGuest = false;
    public bool IsGuest => m_isGuest;

    private LoadingScreen? m_loadingScreen = null;

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
        //InitializeGameAsync();
    }

    private async void InitializeGameAsync()
    {
        m_status = AppStatus.Loading;
        GD.Print("[GameManager] Initializing game...");

        ShowLoadingScreen();

        IApiService apiService = ServiceRegistry.Instance.ApiService;
        string? storedToken = SessionProvider.GetStoredToken();

        if (!string.IsNullOrEmpty(storedToken))
        {
            GD.Print("[GameManager] Found stored token, validating...");
            m_loadingScreen?.ShowLoading("Validation de la session...");
            apiService.SetSessionToken(storedToken);

            try
            {
                ProfileResponse? profileResponse = await apiService.GetProfileAsync();
                if (profileResponse != null)
                {
                    GD.Print("[GameManager] Session validated successfully. Mapping profile...");
                    m_loadingScreen?.ShowLoading("Synchronisation du profil...");
                    PlayerProfile profile = ProfileMapper.MapToDomain(profileResponse);

                    GD.Print("[GameManager] Publishing ProfileLoadedEvent.");
                    ServiceRegistry.Instance.EventBus.Publish(new ProfileLoadedEvent(profile));

                    await CompleteInitialization("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
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
                HandleInitializationError(ex);
                return;
            }
        }

        GD.Print("[GameManager] No valid session. Redirecting to Login.");
        await CompleteInitialization("res://Scenes/Login/Login.tscn");
    }

    private void ShowLoadingScreen()
    {
        if (m_loadingScreen != null) return;

        PackedScene loadingScene = GD.Load<PackedScene>("res://Scenes/UI/LoadingScreen/LoadingScreen.tscn");
        m_loadingScreen = loadingScene.Instantiate<LoadingScreen>();

        m_loadingScreen.RetryRequested += RetryInitialization;
        m_loadingScreen.OfflineModeRequested += SetGuestMode;

        AddChild(m_loadingScreen);

        // Lock current scene input/process
        Node activeScene = GetTree().CurrentScene;
        if (activeScene != null)
        {
            activeScene.ProcessMode = ProcessModeEnum.Disabled;
        }
    }

    private async Task CompleteInitialization(string p_targetScene)
    {
        m_status = AppStatus.Ready;
        m_loadingScreen?.ShowLoading("Données chargées");

        // Brief delay for user feedback
        await Task.Delay(500);

        GetTree().ChangeSceneToFile(p_targetScene);

        // We don't restore ProcessMode here because ChangeSceneToFile will load a new scene
        // with its default ProcessMode (Inherit).
        // If we were staying on the same scene, we would restore it.

        HideLoadingScreen();
    }

    private void HideLoadingScreen()
    {
        if (m_loadingScreen != null)
        {
            m_loadingScreen.QueueFree();
            m_loadingScreen = null;
        }
    }

    private void HandleInitializationError(System.Exception p_ex)
    {
        string message = "Serveur indisponible ou problème de connexion.";

        // In a real scenario, we'd check the exception type or status code.
        // Assuming we have a way to detect 401:
        if (p_ex.Message.Contains("401") || p_ex.Message.Contains("Unauthorized"))
        {
            message = "Session expirée ou invalide. Veuillez vous reconnecter.";
        }

        m_loadingScreen?.ShowError(message);
    }

    public async void SetGuestMode()
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

        await CompleteInitialization("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
    }

    public void RetryInitialization()
    {
        GD.Print("[GameManager] Retrying initialization...");
        InitializeGameAsync();
    }
}
