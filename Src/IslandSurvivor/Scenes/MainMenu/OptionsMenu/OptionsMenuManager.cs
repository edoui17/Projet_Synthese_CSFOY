using Godot;
using System;

public partial class OptionsMenuManager : Control
{
    [Export] public Button _returnToMenuButton = null!;

    public override void _Ready()
    {
        _returnToMenuButton.Pressed += OnReturnToMenuPressed;
    }

    public void OnReturnToMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
    }
}
