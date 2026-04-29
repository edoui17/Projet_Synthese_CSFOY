using Godot;
using System;
using System.Resources;

using Core.Interfaces;

public partial class InventoryUi : Godot.Control
{
    [Export] public RessourceSlot WoodSlot;
    [Export] public RessourceSlot StoneSlot;
    [Export] public RessourceSlot FoodSlot;
    [Export] public RessourceSlot GoldSlot;

    [Export] public Texture2D WoodTexture;
    [Export] public Texture2D StoneTexture;
    [Export] public Texture2D FoodTexture;
    [Export] public Texture2D GoldTexture;

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
        // e.g. "tree_conifer_01" and "tree_automn_01" -> we can track them individually or you might have a generic type
        // The current implementation in resources uses EntityId: "tree_conifer_01", "rock_01", "meat_01", "gold_01"
        // Update: Let's fetch all slots and update based on type or ID

        int woodCount = manager.GetMaterialCount("tree_conifer_01") + manager.GetMaterialCount("tree_automn_01") + manager.GetMaterialCount("Bois"); // Adding "Bois" in case that ID is used
        int stoneCount = manager.GetMaterialCount("rock_01") + manager.GetMaterialCount("Roche");
        int foodCount = manager.GetMaterialCount("meat_01") + manager.GetMaterialCount("Viande");
        int goldCount = manager.GetMaterialCount("gold_01") + manager.GetMaterialCount("gold_coin") + manager.GetMaterialCount("Or");

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
