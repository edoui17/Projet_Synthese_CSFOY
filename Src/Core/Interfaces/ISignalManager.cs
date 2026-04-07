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
}
