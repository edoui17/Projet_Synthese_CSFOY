using Godot;
using System;
using Core.Interfaces;
using Core.Managers;

public partial class InventoryNode : Node
{
    private IInventoryManager m_inventoryManager;

    public override void _Ready()
    {
        m_inventoryManager = new InventoryManager();

        // Connect to the global SignalManager
        SignalManager.Instance.OnMaterialDestroyed.AddListener(OnMaterialDestroyed);

        GD.Print("InventoryNode ready. Listening for material destruction events.");
    }

    private void OnMaterialDestroyed(object p_sender, ISignalManager.MaterialDestroyedEventArgs p_args)
    {
        m_inventoryManager.AddMaterial(p_args.MaterialType, p_args.MaterialQuantity);
        GD.Print($"[Inventory] Added {p_args.MaterialQuantity} of {p_args.MaterialType}. Total {p_args.MaterialType}: {m_inventoryManager.GetMaterialCount(p_args.MaterialType)}");
    }

    public override void _ExitTree()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnMaterialDestroyed.RemoveListener(OnMaterialDestroyed);
        }
    }
}
