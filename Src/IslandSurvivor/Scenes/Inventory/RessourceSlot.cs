using Godot;
using System;

public partial class RessourceSlot : Control
{
    [Export] public TextureRect IconTextureRect = null!;
    [Export] public Label AmountLabel = null!;

    public void SetIcon(Texture2D texture)
    {
        IconTextureRect.Texture = texture;
    }

    public void SetAmount(int amount)
    {
        AmountLabel.Text = amount.ToString();
    }
}
