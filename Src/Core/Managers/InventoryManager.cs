namespace Core.Managers;

using System.Collections.Generic;
using System.Linq;
using Core.Domain;
using Core.Domain.Models;
using Core.Events;
using Core.Interfaces;

public class InventoryManager : IInventoryManager
{
    private readonly Dictionary<string, InventorySlot> m_slots = new Dictionary<string, InventorySlot>();
    private readonly IEventBus m_eventBus;

    public InventoryManager(IEventBus p_eventBus)
    {
        m_eventBus = p_eventBus;

        m_eventBus.Subscribe<NavigationRequestedEvent>(OnNavigationRequested);
        m_eventBus.Subscribe<PurchaseAttemptedEvent>(OnPurchaseAttempted);
        m_eventBus.Subscribe<ResourceHarvestedEvent>(OnResourceHarvested);
        m_eventBus.Subscribe<ResourceSpentEvent>(OnResourceSpent);
    }

    private void OnNavigationRequested(NavigationRequestedEvent p_event)
    {
        IslandDestination destination = p_event.Destination;

        if (destination.Id == IslandDestination.HomeIsland.Id)
        {
            m_eventBus.Publish(new NavigationApprovedEvent(destination));
            return;
        }

        int requiredCost = destination.ResourceCost;

        if (requiredCost > 0)
        {
            bool hasMeat = GetMaterialCount("meat_01") >= requiredCost;
            bool hasWood = GetMaterialCount("wood_01") >= requiredCost;
            bool hasRock = GetMaterialCount("rock_01") >= requiredCost;
            bool hasGold = GetMaterialCount("gold_01") >= requiredCost;

            if (!hasMeat || !hasWood || !hasRock || !hasGold)
            {
                m_eventBus.Publish(new NavigationRejectedEvent(destination, "Insufficient Resources"));
                return;
            }

            // Deduct items and emit spent events
            RemoveMaterial("meat_01", requiredCost);
            m_eventBus.Publish(new ResourceSpentEvent("meat_01", requiredCost));

            RemoveMaterial("wood_01", requiredCost);
            m_eventBus.Publish(new ResourceSpentEvent("wood_01", requiredCost));

            RemoveMaterial("rock_01", requiredCost);
            m_eventBus.Publish(new ResourceSpentEvent("rock_01", requiredCost));

            RemoveMaterial("gold_01", requiredCost);
            m_eventBus.Publish(new ResourceSpentEvent("gold_01", requiredCost));
        }

        m_eventBus.Publish(new NavigationApprovedEvent(destination));
    }

    private void OnPurchaseAttempted(PurchaseAttemptedEvent p_event)
    {
        if (GetMaterialCount(p_event.ItemId) >= p_event.Cost)
        {
            RemoveMaterial(p_event.ItemId, p_event.Cost);
            m_eventBus.Publish(new ResourceSpentEvent(p_event.ItemId, p_event.Cost));
            m_eventBus.Publish(new TransactionResultEvent(p_event.ItemId, true, "Success"));
        }
        else
        {
            m_eventBus.Publish(new TransactionResultEvent(p_event.ItemId, false, "Insufficient Resources"));
        }
    }

    private void OnResourceHarvested(ResourceHarvestedEvent p_event)
    {
        // Handled via AddMaterial locally, but if another manager emits this, we add it here.
        // Assuming default type and empty icon for now, ideally this would be fetched from a data repository.
        AddMaterial(new ResourceItem(p_event.ResourceId, p_event.ResourceId, "Resource", ""), p_event.Amount);
    }

    private void OnResourceSpent(ResourceSpentEvent p_event)
    {
        // When a resource is spent (deducted successfully), notify UI
        m_eventBus.Publish(new InventoryChangedEvent(p_event.ResourceId, GetMaterialCount(p_event.ResourceId)));
    }

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

        m_eventBus.Publish(new InventoryChangedEvent(p_item.Id, m_slots[p_item.Id].Quantity));
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
