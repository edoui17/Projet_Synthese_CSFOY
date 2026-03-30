using Godot;
using System;
using Core.Interfaces;
using Core.Managers;

public partial class InventoryNode : Node
{
    public static InventoryNode Instance { get; private set; }

    private IInventoryManager m_inventoryManager;
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
        m_inventoryManager = new InventoryManager();

        // Connect to the global SignalManager
        SignalManager.Instance.OnMaterialDestroyed.AddListener(OnMaterialDestroyed);

        GD.Print("InventoryNode ready. Listening for material destruction events.");
    }

    private void OnMaterialDestroyed(object p_sender, ISignalManager.MaterialDestroyedEventArgs p_args)
    {
        m_inventoryManager.AddMaterial(p_args.Item, p_args.MaterialQuantity);
        GD.Print($"[Inventory] Added {p_args.MaterialQuantity} of {p_args.Item.Name} ({p_args.Item.Id}). Total: {m_inventoryManager.GetMaterialCount(p_args.Item.Id)}");
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
            SignalManager.Instance.OnMaterialDestroyed.RemoveListener(OnMaterialDestroyed);
        }
    }
}
