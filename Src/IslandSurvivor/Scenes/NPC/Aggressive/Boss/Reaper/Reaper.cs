using Godot;
using System.Linq;

namespace IslandSurvivor.Scenes.NPC.Aggressive;

public partial class Reaper : BossBase
{
    [ExportGroup("Reaper Animations")]
    [Export] public string NormalAttackAnimationName { get; set; } = "MeleeAttackNormal";
    [Export] public string EnragedAttackAnimationName { get; set; } = "MeleeAttackEnraged";

    private bool m_isAnimationTestMode = false;
    private int m_currentAnimationIndex = 0;
    private string[] m_animationNames;

    private Tween m_colorTween;
    private Tween m_hoverTween;
    private Vector2 m_originalSpritePosition;

    public override void _Ready()
    {
        base._Ready();

        if (m_sprite != null)
        {
            m_originalSpritePosition = m_sprite.Position;
        }

        if (m_animationPlayer != null)
        {
            var animList = m_animationPlayer.GetAnimationList();
            m_animationNames = new string[animList.Length];
            for (int i = 0; i < animList.Length; i++)
            {
                m_animationNames[i] = animList[i].ToString();
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.T)
            {
                m_isAnimationTestMode = !m_isAnimationTestMode;
                GD.Print($"[Reaper] Animation Test Mode: {(m_isAnimationTestMode ? "ON" : "OFF")}");

                if (m_isAnimationTestMode && m_animationNames != null && m_animationNames.Length > 0)
                {
                    PlayTestAnimation();
                }
                else if (!m_isAnimationTestMode)
                {
                    ResetAnimationEffects();
                }
            }
            else if (m_isAnimationTestMode && keyEvent.Keycode == Key.Y)
            {
                if (m_animationNames != null && m_animationNames.Length > 0)
                {
                    m_currentAnimationIndex = (m_currentAnimationIndex + 1) % m_animationNames.Length;
                    PlayTestAnimation();
                }
            }
        }
    }

    private void PlayTestAnimation()
    {
        if (m_animationPlayer == null) return;

        string animName = m_animationNames[m_currentAnimationIndex];
        GD.Print($"[Reaper] Playing Animation: {animName}");

        ResetAnimationEffects();

        m_animationPlayer.Play(animName);

        if (animName == "IdleShielded")
        {
            ApplyShieldEffects();
        }
    }

    private void ApplyShieldEffects()
    {
        if (m_sprite == null) return;

        // Reset any existing tweens just in case
        ResetAnimationEffects();

        // 1. Light Blue Modulation Flashing
        m_colorTween = CreateTween();
        m_colorTween.SetLoops(); // Loop infinitely
        Color lightBlue = new Color(0.5f, 0.8f, 1.0f, 1.0f); // Light blue tint
        m_colorTween.TweenProperty(m_sprite, "modulate", lightBlue, 1.0f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        m_colorTween.TweenProperty(m_sprite, "modulate", Colors.White, 1.0f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

        // 2. Slow Hovering Effect
        m_hoverTween = CreateTween();
        m_hoverTween.SetLoops();
        Vector2 upPos = m_originalSpritePosition + new Vector2(0, -15);
        Vector2 downPos = m_originalSpritePosition + new Vector2(0, 5);

        m_hoverTween.TweenProperty(m_sprite, "position", upPos, 2.0f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        m_hoverTween.TweenProperty(m_sprite, "position", downPos, 2.0f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
    }

    private void ResetAnimationEffects()
    {
        if (m_colorTween != null)
        {
            m_colorTween.Kill();
            m_colorTween = null;
        }
        if (m_hoverTween != null)
        {
            m_hoverTween.Kill();
            m_hoverTween = null;
        }

        if (m_sprite != null)
        {
            m_sprite.Modulate = Colors.White;
            m_sprite.Position = m_originalSpritePosition;
        }
    }

    public override void _PhysicsProcess(double p_delta)
    {
        if (m_isAnimationTestMode)
        {
            // Do not run normal AI or movement when testing animations
            Velocity = Vector2.Zero;
            return;
        }

        // Run normal boss physics process
        base._PhysicsProcess(p_delta);
    }


    protected override void OnAttackStarted()
    {
        // Handled via State Machine
    }
}
