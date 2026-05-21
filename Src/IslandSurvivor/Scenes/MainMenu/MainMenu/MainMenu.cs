using Godot;
using System;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        var startBtn = GetNodeOrNull<TextureButton>("Panel/MenuTab/VBoxContainer/StartGameBtn");
        var scoreBtn = GetNodeOrNull<TextureButton>("Panel/MenuTab/VBoxContainer/ScoreboardBtn");
        var optionsBtn = GetNodeOrNull<TextureButton>("Panel/MenuTab/VBoxContainer/OptionsBtn");
        var quitBtn = GetNodeOrNull<TextureButton>("Panel/MenuTab/VBoxContainer/QuitGameBtn");

        if (startBtn != null && scoreBtn != null && optionsBtn != null && quitBtn != null)
        {
            SetupButton(startBtn);
            SetupButton(scoreBtn);
            SetupButton(optionsBtn);
            SetupButton(quitBtn);

            // Focus looping wrap-around
            startBtn.FocusNeighborTop = quitBtn.GetPath();
            quitBtn.FocusNeighborBottom = startBtn.GetPath();

            startBtn.GrabFocus();
        }
    }

    private void SetupButton(TextureButton btn)
    {
        btn.FocusMode = FocusModeEnum.All;
        btn.MouseDefaultCursorShape = CursorShape.PointingHand;

        // Pivot point at center for nice scaling
        btn.PivotOffset = btn.Size / 2;

        btn.MouseEntered += () => btn.GrabFocus();

        btn.FocusEntered += () =>
        {
            Tween tween = CreateTween();
            tween.TweenProperty(btn, "scale", new Vector2(1.05f, 1.05f), 0.1f);
        };

        btn.FocusExited += () =>
        {
            Tween tween = CreateTween();
            tween.TweenProperty(btn, "scale", new Vector2(1.0f, 1.0f), 0.1f);
        };
    }

    private void _on_start_game_btn_pressed()
    {
        GD.Print("Bouton start appuyé");
        GetTree().ChangeSceneToFile("res://Scenes/Level/PlayerHub/PlayerHub.tscn");

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

    private void _on_quit_game_btn_pressed()
    {
        GD.Print("Bouton quit appuyé");
        GetTree().Quit();
    }
}
