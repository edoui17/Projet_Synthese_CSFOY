using Godot;
using System;
using System.Resources;

using Core.Interfaces;

public partial class InventoryUi : Godot.Control
{
    [Export] public RessourceSlot WoodSlot = null!;
    [Export] public RessourceSlot StoneSlot = null!;
    [Export] public RessourceSlot FoodSlot = null!;
    [Export] public RessourceSlot GoldSlot = null!;

    [Export] public Texture2D WoodTexture = null!;
    [Export] public Texture2D StoneTexture = null!;
    [Export] public Texture2D FoodTexture = null!;
    [Export] public Texture2D GoldTexture = null!;

    public override void _Ready()
    {
        WoodSlot.SetIcon(WoodTexture);
        StoneSlot.SetIcon(StoneTexture);
        FoodSlot.SetIcon(FoodTexture);
        GoldSlot.SetIcon(GoldTexture);

        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.InventoryChanged += OnInventoryChanged;
        }

        UpdateAllSlots();
    }

    private void UpdateAllSlots()
    {
        if (InventoryNode.Instance == null || InventoryNode.Instance.Manager == null) return;

        IInventoryManager manager = InventoryNode.Instance.Manager;

        // Ensure these IDs match the EntityId exported in the Resource scripts
        // e.g. "wood_01" -> we can track them individually or you might have a generic type
        // The current implementation in resources uses EntityId: "wood_01", "rock_01", "meat_01", "gold_01"
        // Update: Let's fetch all slots and update based on type or ID

        int woodCount = manager.GetMaterialCount("wood_01");
        int stoneCount = manager.GetMaterialCount("rock_01");
        int foodCount = manager.GetMaterialCount("meat_01");
        int goldCount = manager.GetMaterialCount("gold_01");

        WoodSlot.SetAmount(woodCount);
        StoneSlot.SetAmount(stoneCount);
        FoodSlot.SetAmount(foodCount);
        GoldSlot.SetAmount(goldCount);
    }

    private void OnInventoryChanged(string p_resourceId, int p_totalAmount)
    {
        UpdateAllSlots();
    }

    public override void _ExitTree()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.InventoryChanged -= OnInventoryChanged;
        }
        base._ExitTree();
    }
}
