namespace Core.Interfaces;

using Core.Managers;

public interface IShopManager
{
    int CalculateCost(int p_upgradeCount);
    bool CanAffordUpgrade(IInventoryManager p_inventoryManager, string p_resourceId, int p_cost);
    bool CanAffordIsland(IInventoryManager p_inventoryManager, int p_cost);
}
