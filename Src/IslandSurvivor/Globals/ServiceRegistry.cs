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
    public static ServiceRegistry Instance { get; private set; } = null!;

    public IInventoryManager InventoryManager { get; private set; } = null!;
    public IShopManager ShopManager { get; private set; } = null!;
    public IScoreTracker ScoreTracker { get; private set; } = null!;
    public INavigationService NavigationService { get; private set; } = null!;
    public IEventBus EventBus { get; private set; } = null!;
    public IApiService ApiService { get; private set; } = null!;
    public IStatTracker StatTracker { get; private set; } = null!;
    public ISaveService SaveService { get; private set; } = null!;

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

        StatTracker = new StatTracker(EventBus);
        InventoryManager = new InventoryManager(EventBus);
        ShopManager = new ShopManager(EventBus);

        // Pass Godot specific implementation of ISaveService
        SaveService = new GodotSaveService();
        ScoreTracker = new ScoreTracker(SaveService, EventBus);
        NavigationService = new NavigationService(EventBus, ShopManager, InventoryManager);

        string apiKey = ProjectSettings.GetSetting("network/api/api_key").AsString();
        ApiService = new ApiService(SaveService, apiKey);
    }

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        EventBus?.ProcessEvents();
    }
}
