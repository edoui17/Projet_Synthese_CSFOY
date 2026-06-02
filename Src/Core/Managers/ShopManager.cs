namespace Core.Managers;

using System;
using Core.Events;
using Core.Interfaces;

public class ShopManager : IShopManager
{
    private readonly IEventBus m_eventBus;

    public ShopManager(IEventBus p_eventBus)
    {
        m_eventBus = p_eventBus;

        // Example logic to handle successful purchases if ShopManager needed state
        m_eventBus.Subscribe<TransactionResultEvent>(OnTransactionResult);
    }

    private void OnTransactionResult(TransactionResultEvent p_event)
    {
        // Internal logic for when a transaction completes (success/failure)
    }

    public int CalculateCost(int p_upgradeCount)
    {
        // Cost scaling: 1, 3, 7, 15... (2^(n+1) - 1)
        return (int)Math.Pow(2, p_upgradeCount + 1) - 1;
    }

    public bool CanAffordUpgrade(IInventoryManager p_inventoryManager, string p_resourceId, int p_cost)
    {
        if (p_inventoryManager == null || string.IsNullOrEmpty(p_resourceId))
        {
            return false;
        }

        int currentAmount = p_inventoryManager.GetMaterialCount(p_resourceId);
        return currentAmount >= p_cost;
    }

    public bool CanAffordIsland(IInventoryManager p_inventoryManager, int p_cost)
    {
        if (p_inventoryManager == null)
        {
            return false;
        }

        int meatCount = p_inventoryManager.GetMaterialCount(Core.Constants.ResourceConstants.MEAT);
        int woodCount = p_inventoryManager.GetMaterialCount(Core.Constants.ResourceConstants.WOOD);
        int rockCount = p_inventoryManager.GetMaterialCount(Core.Constants.ResourceConstants.ROCK);
        int goldCount = p_inventoryManager.GetMaterialCount(Core.Constants.ResourceConstants.GOLD);

        return meatCount >= p_cost && woodCount >= p_cost && rockCount >= p_cost && goldCount >= p_cost;
    }
}
