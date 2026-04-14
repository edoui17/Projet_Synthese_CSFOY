namespace Core.Interfaces;

using System;
using Core.Utils;

public interface ISignalManager
{
    // === EXAMPLE OF HOW TO IMPLEMENT A SIGNAL ===
    //
    // // 1. Define the strongly-typed WeakEvent property
    // WeakEvent<ScoreChangedEventArgs> OnScoreChanged { get; }
    //
    // // 2. Define the emit method
    // void EmitScoreChanged(object p_sender, int p_newScore);
    //
    // // 3. Define the EventArgs class
    // public class ScoreChangedEventArgs : EventArgs
    // {
    //     public int NewScore { get; }
    //     public ScoreChangedEventArgs(int p_newScore) => NewScore = p_newScore;
    // }

    public class MaterialDestroyedEventArgs : EventArgs
    {
        public Core.Domain.ResourceItem Item { get; }
        public int MaterialQuantity { get; }

        public MaterialDestroyedEventArgs(Core.Domain.ResourceItem p_item, int p_quantity)
        {
            Item = p_item;
            MaterialQuantity = p_quantity;
        }
    }

    WeakEvent<MaterialDestroyedEventArgs> OnMaterialDestroyed { get; }
    void EmitMaterialDestroyed(object p_sender, Core.Domain.ResourceItem p_item, int p_quantity);

    public class ResourceSpentEventArgs : EventArgs
    {
        public string ResourceId { get; }
        public int Amount { get; }

        public ResourceSpentEventArgs(string p_resourceId, int p_amount)
        {
            ResourceId = p_resourceId;
            Amount = p_amount;
        }
    }

    WeakEvent<ResourceSpentEventArgs> OnResourceSpent { get; }
    void EmitResourceSpent(object p_sender, string p_resourceId, int p_amount);

    public class StatUpgradePurchasedEventArgs : EventArgs
    {
        public Core.Managers.Stats.StatType StatType { get; }

        public StatUpgradePurchasedEventArgs(Core.Managers.Stats.StatType p_statType)
        {
            StatType = p_statType;
        }
    }

    WeakEvent<StatUpgradePurchasedEventArgs> OnStatUpgradePurchased { get; }
    void EmitStatUpgradePurchased(object p_sender, Core.Managers.Stats.StatType p_statType);

    public class NavigationRequestedEventArgs : EventArgs
    {
        public Core.Domain.Models.IslandDestination Destination { get; }

        public NavigationRequestedEventArgs(Core.Domain.Models.IslandDestination p_destination)
        {
            Destination = p_destination;
        }
    }

    WeakEvent<NavigationRequestedEventArgs> OnNavigationRequested { get; }
    void EmitNavigationRequested(object p_sender, Core.Domain.Models.IslandDestination p_destination);

    public class BuildingShopToggledEventArgs : EventArgs
    {
        public bool IsOpen { get; }
        public string BuildingId { get; }

        public BuildingShopToggledEventArgs(bool p_isOpen, string p_buildingId)
        {
            IsOpen = p_isOpen;
            BuildingId = p_buildingId;
        }
    }

    WeakEvent<BuildingShopToggledEventArgs> OnBuildingShopToggled { get; }
    void EmitBuildingShopToggled(object p_sender, bool p_isOpen, string p_buildingId);
}
