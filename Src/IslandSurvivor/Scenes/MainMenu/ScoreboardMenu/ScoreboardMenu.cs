using Godot;
using System;

public partial class ScoreboardMenu : Control
{
    private Button m_returnBtn = null!;

    public override void _Ready()
    {
        m_returnBtn = GetNode<Button>("VBoxContainer/ReturnMenuBtn");

        SetupButtonJuice(m_returnBtn);

        // Grab focus so keyboard navigation works immediately
        m_returnBtn.GrabFocus();
    }

    private void SetupButtonJuice(Button p_btn)
    {
        p_btn.FocusMode = FocusModeEnum.All;
        p_btn.MouseDefaultCursorShape = CursorShape.PointingHand;

        // Support keyboard tracking smoothly
        p_btn.MouseEntered += () => p_btn.GrabFocus();

        p_btn.FocusEntered += () =>
        {
            if (p_btn.HasMeta("tween"))
            {
                p_btn.GetMeta("tween").As<Tween>()?.Kill();
            }
            Tween tween = CreateTween();
            p_btn.SetMeta("tween", tween);
            p_btn.PivotOffset = p_btn.Size / 2f;
            tween.TweenProperty(p_btn, "scale", new Vector2(1.05f, 1.05f), 0.1f);
        };

        p_btn.FocusExited += () =>
        {
            if (p_btn.HasMeta("tween"))
            {
                p_btn.GetMeta("tween").As<Tween>()?.Kill();
            }
            Tween tween = CreateTween();
            p_btn.SetMeta("tween", tween);
            p_btn.PivotOffset = p_btn.Size / 2f;
            tween.TweenProperty(p_btn, "scale", new Vector2(1.0f, 1.0f), 0.1f);
        };
    }

    public void _on_return_menu_btn_pressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu/MainMenu/MainMenu.tscn");
    }
}
