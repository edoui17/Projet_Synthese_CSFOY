using Godot;
using System;

public partial class MainMenu : Control
{
    [Export] public Button _startGameButton;
    [Export] public Button _optionsGameButton;
    [Export] public Button _quitGameButton;

    public override void _Ready()
    {
        _startGameButton.Pressed += OnBtnStartGamePressed;
        _optionsGameButton.Pressed += OnBtnOptionsGamePressed;
        _quitGameButton.Pressed += OnBtnQuitGamePressed;
    }

    private void OnBtnStartGamePressed()
    {
        GD.Print("Bouton start appuyé");
    }

    private void OnBtnOptionsGamePressed()
    {
        GD.Print("Bouton options appuyé");
    }

    private void OnBtnQuitGamePressed()
    {
        GD.Print("Bouton quit appuyé");
    }
}
