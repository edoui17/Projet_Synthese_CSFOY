using Godot;
using Core.Interfaces;
using Core.Interfaces.Stats;
using Core.Interfaces.Navigation;
using Core.Managers;
using Core.Managers.Stats;
using Core.Managers.Navigation;
using Core.Services;

namespace IslandSurvivor.Globals;

public partial class ServiceRegistry : Node
{
  public static ServiceRegistry Instance { get; private set; }

  public IInventoryManager InventoryManager { get; private set; }
  public IShopManager ShopManager { get; private set; }
  public IStatTracker StatTracker { get; private set; }
  public IScoreTracker ScoreTracker { get; private set; }
  public INavigationService NavigationService { get; private set; }
  public IEventBus EventBus { get; private set; }

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
    StatTracker = new StatTracker(EventBus);

    // Pass Godot specific implementation of ISaveService
    ISaveService saveService = new GodotSaveService();
    ScoreTracker = new ScoreTracker(saveService, EventBus);
    NavigationService = new NavigationService(EventBus, ShopManager, InventoryManager);
  }

  public override void _Process(double delta)
  {
      EventBus?.ProcessEvents();
  }
}
