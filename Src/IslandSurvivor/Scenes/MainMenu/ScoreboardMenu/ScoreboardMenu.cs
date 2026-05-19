using Godot;
using System;

public partial class ScoreboardMenu : Control
{
    public override void _Ready()
    {
        //
    }

    public void _on_return_menu_btn_pressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
    }
}
