using Godot;
using System;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        //
    }

    private void _on_start_game_btn_pressed()
    {
        GD.Print("Bouton start appuyé");
        GetTree().ChangeSceneToFile("res://Scenes/LaSceneMain/main.tscn");

    }

    private void _on_scoreboard_btn_pressed()
    {
        GD.Print("Bouton score appuyé");
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu/ScoreboardMenu/ScoreboardMenu.tscn");
    }

    private void _on_options_btn_pressed()
    {
        GD.Print("Bouton options appuyé");
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu/OptionsMenu/OptionsMenu.tscn");
    }

    private void _on_quitgame_btn_pressed()
    {
        GD.Print("Bouton quit appuyé");
        GetTree().Quit();
    }
}
