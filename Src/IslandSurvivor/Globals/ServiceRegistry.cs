using Godot;
using Core.Interfaces;
using Core.Interfaces.Stats;
using Core.Interfaces.Navigation;
using Core.Managers;
using Core.Managers.Stats;
using Core.Managers.Navigation;
using Core.Services;
using Core.Domain;
using Core.Events;

namespace IslandSurvivor.Globals;

public partial class ServiceRegistry : Node
{
  public static ServiceRegistry Instance { get; private set; }

  public IInventoryManager InventoryManager { get; private set; }
  public IShopManager ShopManager { get; private set; }
  public IScoreTracker ScoreTracker { get; private set; }
  public INavigationService NavigationService { get; private set; }
  public IEventBus EventBus { get; private set; }
  public IApiService ApiService { get; private set; }

  public override void _EnterTree()
  {
    if (Instance != null)
    {
      QueueFree();
      return;
    }

    Instance = this;

    // Initialize Core Managers
    EventBus = new EventBus();

    InventoryManager = new InventoryManager(EventBus);
    ShopManager = new ShopManager(EventBus);

    // Pass Godot specific implementation of ISaveService
    ISaveService saveService = new GodotSaveService();
    ScoreTracker = new ScoreTracker(saveService, EventBus);
    NavigationService = new NavigationService(EventBus, ShopManager, InventoryManager);

    ApiService = new ApiService(saveService);
  }

  public override void _Ready()
  {
      CallDeferred(nameof(InitializeApiData));
  }

  private async void InitializeApiData()
  {
      GD.Print("[ServiceRegistry] Connecting to API...");

      // Use temporary test credentials
      string? token = await ApiService.LoginAsync("test", "test");
      if (string.IsNullOrEmpty(token))
      {
          GD.Print("[ServiceRegistry] API Login failed, falling back to local cache if available.");
      }
      else
      {
          GD.Print("[ServiceRegistry] API Login successful.");
      }

      PlayerProfile? profile = await ApiService.GetProfileAsync();
      if (profile != null)
      {
          GD.Print("[ServiceRegistry] Profile data loaded.");
          EventBus.Publish(new ProfileLoadedEvent(profile));
      }
  }

  public override void _Process(double delta)
  {
      EventBus?.ProcessEvents();
  }
}
