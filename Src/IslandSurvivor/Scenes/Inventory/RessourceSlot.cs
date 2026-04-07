using Godot;
using System;

public partial class RessourceSlot : Control
{
    [Export] public TextureRect IconTextureRect;
    [Export] public Label AmountLabel;

    public void SetIcon(Texture2D texture)
    {
        IconTextureRect.Texture = texture;
    }

    public void SetAmount(int amount)
    {
        AmountLabel.Text = amount.ToString();
    }
}
