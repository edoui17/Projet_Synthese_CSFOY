using Godot;
using System;
using Core.Interfaces;
using Core.Managers;
using Core.Utils;

public partial class SignalManager : Node, ISignalManager
{
    private static SignalManager m_instance;
    private readonly SignalManagerCore m_coreManager = new SignalManagerCore();

    public static SignalManager Instance => m_instance;

    public override void _EnterTree()
    {
        if (m_instance != null)
        {
            QueueFree();
            return;
        }

        m_instance = this;
    }

    public WeakEvent<ISignalManager.MaterialDestroyedEventArgs> OnMaterialDestroyed => m_coreManager.OnMaterialDestroyed;

    public void EmitMaterialDestroyed(object p_sender, Core.Domain.ResourceItem p_item, int p_quantity)
    {
        m_coreManager.EmitMaterialDestroyed(p_sender, p_item, p_quantity);
    }

    public WeakEvent<ISignalManager.ResourceSpentEventArgs> OnResourceSpent => m_coreManager.OnResourceSpent;

    public void EmitResourceSpent(object p_sender, string p_resourceId, int p_amount)
    {
        m_coreManager.EmitResourceSpent(p_sender, p_resourceId, p_amount);
    }

    public WeakEvent<ISignalManager.StatUpgradePurchasedEventArgs> OnStatUpgradePurchased => m_coreManager.OnStatUpgradePurchased;

    public void EmitStatUpgradePurchased(object p_sender, Core.Managers.Stats.StatType p_statType)
    {
        m_coreManager.EmitStatUpgradePurchased(p_sender, p_statType);
    }

    // === EXAMPLE OF HOW TO IMPLEMENT A SIGNAL IN GODOT ===
    //
    // // 1. Map to the Core implementation
    // public WeakEvent<ISignalManager.ScoreChangedEventArgs> OnScoreChanged => m_coreManager.OnScoreChanged;
    //
    // // 2. Delegate the emit method
    // public void EmitScoreChanged(object p_sender, int p_newScore)
    // {
    //     m_coreManager.EmitScoreChanged(p_sender, p_newScore);
    // }
}
