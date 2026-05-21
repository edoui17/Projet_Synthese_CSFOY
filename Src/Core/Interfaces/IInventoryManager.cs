namespace Core.Interfaces;

using Core.Domain;
using System.Collections.Generic;

public interface IInventoryManager
{
    void AddMaterial(ResourceItem p_item, int p_amount);
    void RemoveMaterial(string p_itemId, int p_amount);
    int GetMaterialCount(string p_itemId);
    IReadOnlyList<InventorySlot> GetAllSlots();
    void InitializeInventory(IEnumerable<InventoryEntry> p_entries);
}
