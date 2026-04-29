using Godot;
using System;
using Core.Interfaces;
using Core.Events;
using IslandSurvivor.Globals;

public partial class SignalManager : Node
{
    private static SignalManager m_instance;
    public static SignalManager Instance => m_instance;

    private IEventBus m_eventBus;

    // --- Native Godot Signals (for Godot UI and scene communication) ---
    [Signal] public delegate void MaterialDestroyedEventHandler(string p_itemId, string p_itemName, string p_itemType, string p_itemIcon, int p_quantity);
    [Signal] public delegate void ResourceSpentEventHandler(string p_resourceId, int p_amount);
    [Signal] public delegate void StatUpgradePurchasedEventHandler(int p_statType);
    [Signal] public delegate void NavigationRequestedEventHandler(string p_islandId, string p_scenePath, string p_biome, int p_difficulty, int p_resourceCost, int p_dangerLevel);
    [Signal] public delegate void BuildingShopToggledEventHandler(bool p_isOpen, string p_buildingId);
    [Signal] public delegate void StatChangedEventHandler(int p_statType, float p_currentValue, float p_effectiveMaxValue);

    public override void _EnterTree()
    {
        if (m_instance != null)
        {
            QueueFree();
            return;
        }

        m_instance = this;
    }

    public override void _Ready()
    {
        m_eventBus = ServiceRegistry.Instance.EventBus;

        if (m_eventBus != null)
        {
            // Subscribe to Core Events and re-emit as Godot Signals for Godot-only components
            m_eventBus.Subscribe<MaterialDestroyedEvent>(OnMaterialDestroyedEvent);
            m_eventBus.Subscribe<ResourceSpentEvent>(OnResourceSpentEvent);
            m_eventBus.Subscribe<StatUpgradePurchasedEvent>(OnStatUpgradePurchasedEvent);
            m_eventBus.Subscribe<NavigationRequestedEvent>(OnNavigationRequestedEvent);
            m_eventBus.Subscribe<BuildingShopToggledEvent>(OnBuildingShopToggledEvent);
            m_eventBus.Subscribe<StatChangedEvent>(OnStatChangedEvent);
        }
        else
        {
            GD.PrintErr("SignalManager: EventBus not found in ServiceRegistry!");
        }
    }

    // --- Core -> Godot Bridge ---
    private void OnMaterialDestroyedEvent(MaterialDestroyedEvent e)
    {
        EmitSignal(SignalName.MaterialDestroyed, e.Item.Id, e.Item.Name, e.Item.Type, e.Item.IconPath, e.MaterialQuantity);
    }

    private void OnResourceSpentEvent(ResourceSpentEvent e)
    {
        EmitSignal(SignalName.ResourceSpent, e.ResourceId, e.Amount);
    }

    private void OnStatUpgradePurchasedEvent(StatUpgradePurchasedEvent e)
    {
        EmitSignal(SignalName.StatUpgradePurchased, (int)e.StatType);
    }

    private void OnNavigationRequestedEvent(NavigationRequestedEvent e)
    {
        EmitSignal(SignalName.NavigationRequested, e.Destination.Id, e.Destination.ScenePath, e.Destination.Biome, e.Destination.Difficulty, e.Destination.ResourceCost, e.Destination.DangerLevel);
    }

    private void OnBuildingShopToggledEvent(BuildingShopToggledEvent e)
    {
        EmitSignal(SignalName.BuildingShopToggled, e.IsOpen, e.BuildingId);
    }

    private void OnStatChangedEvent(StatChangedEvent e)
    {
        EmitSignal(SignalName.StatChanged, (int)e.StatType, e.CurrentValue, e.EffectiveMaxValue);
    }

    // --- Godot -> Core Bridge (Proxy methods to emit into Core EventBus) ---
    public void EmitMaterialDestroyed(object p_sender, Core.Domain.ResourceItem p_item, int p_quantity)
    {
        m_eventBus?.Publish(new MaterialDestroyedEvent(p_item, p_quantity));
    }

    public void EmitResourceSpent(object p_sender, string p_resourceId, int p_amount)
    {
        m_eventBus?.Publish(new ResourceSpentEvent(p_resourceId, p_amount));
    }

    public void EmitStatUpgradePurchased(object p_sender, Core.Managers.Stats.StatType p_statType)
    {
        m_eventBus?.Publish(new StatUpgradePurchasedEvent(p_statType));
    }

    public void EmitNavigationRequested(object p_sender, Core.Domain.Models.IslandDestination p_destination)
    {
        m_eventBus?.Publish(new NavigationRequestedEvent(p_destination));
    }

    public void EmitBuildingShopToggled(object p_sender, bool p_isOpen, string p_buildingId)
    {
        m_eventBus?.Publish(new BuildingShopToggledEvent(p_isOpen, p_buildingId));
    }

    protected override void Dispose(bool p_disposing)
    {
        if (p_disposing && m_eventBus != null)
        {
            m_eventBus.Unsubscribe<MaterialDestroyedEvent>(OnMaterialDestroyedEvent);
            m_eventBus.Unsubscribe<ResourceSpentEvent>(OnResourceSpentEvent);
            m_eventBus.Unsubscribe<StatUpgradePurchasedEvent>(OnStatUpgradePurchasedEvent);
            m_eventBus.Unsubscribe<NavigationRequestedEvent>(OnNavigationRequestedEvent);
            m_eventBus.Unsubscribe<BuildingShopToggledEvent>(OnBuildingShopToggledEvent);
            m_eventBus.Unsubscribe<StatChangedEvent>(OnStatChangedEvent);
        }
        base.Dispose(p_disposing);
    }
}
