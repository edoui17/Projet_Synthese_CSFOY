namespace Core.Managers;

using System;
using Core.Interfaces;
using Core.Utils;

public class SignalManagerCore : ISignalManager
{
    private readonly WeakEvent<ISignalManager.MaterialDestroyedEventArgs> m_onMaterialDestroyed = new WeakEvent<ISignalManager.MaterialDestroyedEventArgs>();
    public WeakEvent<ISignalManager.MaterialDestroyedEventArgs> OnMaterialDestroyed => m_onMaterialDestroyed;

    public void EmitMaterialDestroyed(object p_sender, Core.Domain.ResourceItem p_item, int p_quantity)
    {
        m_onMaterialDestroyed.Invoke(p_sender, new ISignalManager.MaterialDestroyedEventArgs(p_item, p_quantity));
    }

    private readonly WeakEvent<ISignalManager.ResourceSpentEventArgs> m_onResourceSpent = new WeakEvent<ISignalManager.ResourceSpentEventArgs>();
    public WeakEvent<ISignalManager.ResourceSpentEventArgs> OnResourceSpent => m_onResourceSpent;

    public void EmitResourceSpent(object p_sender, string p_resourceId, int p_amount)
    {
        m_onResourceSpent.Invoke(p_sender, new ISignalManager.ResourceSpentEventArgs(p_resourceId, p_amount));
    }

    private readonly WeakEvent<ISignalManager.StatUpgradePurchasedEventArgs> m_onStatUpgradePurchased = new WeakEvent<ISignalManager.StatUpgradePurchasedEventArgs>();
    public WeakEvent<ISignalManager.StatUpgradePurchasedEventArgs> OnStatUpgradePurchased => m_onStatUpgradePurchased;

    public void EmitStatUpgradePurchased(object p_sender, Core.Managers.Stats.StatType p_statType)
    {
        m_onStatUpgradePurchased.Invoke(p_sender, new ISignalManager.StatUpgradePurchasedEventArgs(p_statType));
    }

    private readonly WeakEvent<ISignalManager.NavigationRequestedEventArgs> m_onNavigationRequested = new WeakEvent<ISignalManager.NavigationRequestedEventArgs>();
    public WeakEvent<ISignalManager.NavigationRequestedEventArgs> OnNavigationRequested => m_onNavigationRequested;

    public void EmitNavigationRequested(object p_sender, Core.Domain.Models.IslandDestination p_destination)
    {
        m_onNavigationRequested.Invoke(p_sender, new ISignalManager.NavigationRequestedEventArgs(p_destination));
    }

    private readonly WeakEvent<ISignalManager.BuildingShopToggledEventArgs> m_onBuildingShopToggled = new WeakEvent<ISignalManager.BuildingShopToggledEventArgs>();
    public WeakEvent<ISignalManager.BuildingShopToggledEventArgs> OnBuildingShopToggled => m_onBuildingShopToggled;

    public void EmitBuildingShopToggled(object p_sender, bool p_isOpen, string p_buildingId)
    {
        m_onBuildingShopToggled.Invoke(p_sender, new ISignalManager.BuildingShopToggledEventArgs(p_isOpen, p_buildingId));
    }

    // === EXAMPLE OF HOW TO IMPLEMENT A SIGNAL IN CORE ===
    //
    // // 1. Instantiate the WeakEvent
    // private readonly WeakEvent<ISignalManager.ScoreChangedEventArgs> m_onScoreChanged = new WeakEvent<ISignalManager.ScoreChangedEventArgs>();
    //
    // // 2. Expose the public getter
    // public WeakEvent<ISignalManager.ScoreChangedEventArgs> OnScoreChanged => m_onScoreChanged;
    //
    // // 3. Implement the emit method
    // public void EmitScoreChanged(object p_sender, int p_newScore)
    // {
    //     m_onScoreChanged.Invoke(p_sender, new ISignalManager.ScoreChangedEventArgs(p_newScore));
    // }
}
