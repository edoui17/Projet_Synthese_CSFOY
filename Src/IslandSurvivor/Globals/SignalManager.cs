using Godot;
using System;
using Core.Interfaces;
using IslandSurvivor.Globals;

public partial class SignalManager : Node, ISignalManager
{
    private static SignalManager m_instance;

    public static SignalManager Instance => m_instance;

    // --- Native Godot Signals ---
    [Signal] public delegate void MaterialDestroyedEventHandler(string p_itemId, string p_itemName, string p_itemType, string p_itemIcon, int p_quantity);
    [Signal] public delegate void ResourceSpentEventHandler(string p_resourceId, int p_amount);
    [Signal] public delegate void StatUpgradePurchasedEventHandler(int p_statType);
    [Signal] public delegate void NavigationRequestedEventHandler(string p_islandId, string p_scenePath, string p_biome, int p_difficulty, int p_resourceCost, int p_dangerLevel);
    [Signal] public delegate void BuildingShopToggledEventHandler(bool p_isOpen, string p_buildingId);

    // Explicit implementation for Core Interface WeakEvents
    Core.Utils.WeakEvent<ISignalManager.MaterialDestroyedEventArgs> ISignalManager.OnMaterialDestroyed => ServiceRegistry.Instance.SignalManagerCore.OnMaterialDestroyed;
    Core.Utils.WeakEvent<ISignalManager.ResourceSpentEventArgs> ISignalManager.OnResourceSpent => ServiceRegistry.Instance.SignalManagerCore.OnResourceSpent;
    Core.Utils.WeakEvent<ISignalManager.StatUpgradePurchasedEventArgs> ISignalManager.OnStatUpgradePurchased => ServiceRegistry.Instance.SignalManagerCore.OnStatUpgradePurchased;
    Core.Utils.WeakEvent<ISignalManager.NavigationRequestedEventArgs> ISignalManager.OnNavigationRequested => ServiceRegistry.Instance.SignalManagerCore.OnNavigationRequested;
    Core.Utils.WeakEvent<ISignalManager.BuildingShopToggledEventArgs> ISignalManager.OnBuildingShopToggled => ServiceRegistry.Instance.SignalManagerCore.OnBuildingShopToggled;

    // Backward compatibility for refactoring (we will update callers in the next step to use Godot native signals `+=` instead)

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
        // Subscribe to Core WeakEvents and re-emit as Godot Signals
        var coreManager = ServiceRegistry.Instance.SignalManagerCore;
        if (coreManager != null)
        {
            coreManager.OnMaterialDestroyed.AddListener((s, e) =>
                EmitSignal(SignalName.MaterialDestroyed, e.Item.Id, e.Item.Name, e.Item.Type, e.Item.IconPath, e.MaterialQuantity));

            coreManager.OnResourceSpent.AddListener((s, e) =>
                EmitSignal(SignalName.ResourceSpent, e.ResourceId, e.Amount));

            coreManager.OnStatUpgradePurchased.AddListener((s, e) =>
                EmitSignal(SignalName.StatUpgradePurchased, (int)e.StatType));

            coreManager.OnNavigationRequested.AddListener((s, e) =>
                EmitSignal(SignalName.NavigationRequested, e.Destination.Id, e.Destination.ScenePath, e.Destination.Biome, e.Destination.Difficulty, e.Destination.ResourceCost, e.Destination.DangerLevel));

            coreManager.OnBuildingShopToggled.AddListener((s, e) =>
                EmitSignal(SignalName.BuildingShopToggled, e.IsOpen, e.BuildingId));
        }
        else
        {
            GD.PrintErr("SignalManager: SignalManagerCore not found in ServiceRegistry!");
        }
    }

    // Proxy methods to emit into Core
    public void EmitMaterialDestroyed(object p_sender, Core.Domain.ResourceItem p_item, int p_quantity)
    {
        ServiceRegistry.Instance.SignalManagerCore?.EmitMaterialDestroyed(p_sender, p_item, p_quantity);
    }

    public void EmitResourceSpent(object p_sender, string p_resourceId, int p_amount)
    {
        ServiceRegistry.Instance.SignalManagerCore?.EmitResourceSpent(p_sender, p_resourceId, p_amount);
    }

    public void EmitStatUpgradePurchased(object p_sender, Core.Managers.Stats.StatType p_statType)
    {
        ServiceRegistry.Instance.SignalManagerCore?.EmitStatUpgradePurchased(p_sender, p_statType);
    }

    public void EmitNavigationRequested(object p_sender, Core.Domain.Models.IslandDestination p_destination)
    {
        ServiceRegistry.Instance.SignalManagerCore?.EmitNavigationRequested(p_sender, p_destination);
    }

    public void EmitBuildingShopToggled(object p_sender, bool p_isOpen, string p_buildingId)
    {
        ServiceRegistry.Instance.SignalManagerCore?.EmitBuildingShopToggled(p_sender, p_isOpen, p_buildingId);
    }
}
