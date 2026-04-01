namespace Core.Managers;

using System.Collections.Generic;
using System.Linq;
using Core.Domain;
using Core.Interfaces;

public class InventoryManager : IInventoryManager
{
    private readonly Dictionary<string, InventorySlot> m_slots = new Dictionary<string, InventorySlot>();

    public void AddMaterial(ResourceItem p_item, int p_amount)
    {
        if (p_item == null || string.IsNullOrEmpty(p_item.Id) || p_amount <= 0)
        {
            return;
        }

        if (m_slots.ContainsKey(p_item.Id))
        {
            m_slots[p_item.Id].Quantity += p_amount;
        }
        else
        {
            m_slots[p_item.Id] = new InventorySlot(p_item, p_amount);
        }
    }

    public void RemoveMaterial(string p_itemId, int p_amount)
    {
        if (string.IsNullOrEmpty(p_itemId) || p_amount <= 0 || !m_slots.ContainsKey(p_itemId))
        {
            return;
        }

        m_slots[p_itemId].Quantity -= p_amount;
        if (m_slots[p_itemId].Quantity <= 0)
        {
            m_slots.Remove(p_itemId);
        }
    }

    public int GetMaterialCount(string p_itemId)
    {
        if (string.IsNullOrEmpty(p_itemId) || !m_slots.ContainsKey(p_itemId))
        {
            return 0;
        }

        return m_slots[p_itemId].Quantity;
    }

    public IReadOnlyList<InventorySlot> GetAllSlots()
    {
        return m_slots.Values.ToList().AsReadOnly();
    }
}
