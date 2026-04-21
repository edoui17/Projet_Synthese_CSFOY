using Godot;
using System;
using System.Resources;

public partial class InventoryUi : Control
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
    }
}
