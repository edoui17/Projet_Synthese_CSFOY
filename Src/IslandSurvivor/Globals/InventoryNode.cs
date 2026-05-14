using Godot;
using System;
using Core.Interfaces;
using Core.Managers;

public partial class InventoryNode : Node
{
    public static InventoryNode Instance { get; private set; } = null!;

    private IInventoryManager m_inventoryManager = null!;
    public IInventoryManager Manager => m_inventoryManager;

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public override void _Ready()
    {
        m_inventoryManager = IslandSurvivor.Globals.ServiceRegistry.Instance.InventoryManager;

        // Connect to the global SignalManager using native Godot signals
        SignalManager.Instance.MaterialDestroyed += OnMaterialDestroyed;
        SignalManager.Instance.ResourceSpent += OnResourceSpent;

        GD.Print("InventoryNode ready. Listening for material destruction and resource spent events.");
    }

    private void OnMaterialDestroyed(string p_itemId, string p_itemName, string p_itemType, string p_itemIcon, int p_quantity)
    {
        Core.Domain.ResourceItem item = new Core.Domain.ResourceItem(p_itemId, p_itemName, p_itemType, p_itemIcon);
        m_inventoryManager.AddMaterial(item, p_quantity);
        GD.Print($"[Inventory] Added {p_quantity} of {p_itemName} ({p_itemId}). Total: {m_inventoryManager.GetMaterialCount(p_itemId)}");
    }

    private void OnResourceSpent(string p_resourceId, int p_amount)
    {
        ConsumeItem(p_resourceId, p_amount);
    }

    // Example of Consumption for validation purposes (Scenario 3)
    public void ConsumeItem(string p_itemId, int p_amount)
    {
        m_inventoryManager.RemoveMaterial(p_itemId, p_amount);
        GD.Print($"[Inventory] Consumed {p_amount} of {p_itemId}. Total remaining: {m_inventoryManager.GetMaterialCount(p_itemId)}");
    }

    public override void _ExitTree()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.MaterialDestroyed -= OnMaterialDestroyed;
            SignalManager.Instance.ResourceSpent -= OnResourceSpent;
        }
    }
}
