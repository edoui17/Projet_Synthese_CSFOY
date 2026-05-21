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

        if (m_animatedSprite != null && m_animatedSprite.SpriteFrames != null)
        {
            m_animationNames = m_animatedSprite.SpriteFrames.GetAnimationNames();
            m_originalSpritePosition = m_animatedSprite.Position;
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
        if (m_animatedSprite == null) return;

        string animName = m_animationNames[m_currentAnimationIndex];
        GD.Print($"[Reaper] Playing Animation: {animName}");

        ResetAnimationEffects();

        m_animatedSprite.Play(animName);

        if (animName == "IdleShielded")
        {
            ApplyShieldEffects();
        }
    }

    private void ApplyShieldEffects()
    {
        if (m_animatedSprite == null) return;

        // Reset any existing tweens just in case
        ResetAnimationEffects();

        // 1. Light Blue Modulation Flashing
        m_colorTween = CreateTween();
        m_colorTween.SetLoops(); // Loop infinitely
        Color lightBlue = new Color(0.5f, 0.8f, 1.0f, 1.0f); // Light blue tint
        m_colorTween.TweenProperty(m_animatedSprite, "modulate", lightBlue, 1.0f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        m_colorTween.TweenProperty(m_animatedSprite, "modulate", Colors.White, 1.0f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

        // 2. Slow Hovering Effect
        m_hoverTween = CreateTween();
        m_hoverTween.SetLoops();
        Vector2 upPos = m_originalSpritePosition + new Vector2(0, -15);
        Vector2 downPos = m_originalSpritePosition + new Vector2(0, 5);

        m_hoverTween.TweenProperty(m_animatedSprite, "position", upPos, 2.0f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        m_hoverTween.TweenProperty(m_animatedSprite, "position", downPos, 2.0f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
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

        if (m_animatedSprite != null)
        {
            m_animatedSprite.Modulate = Colors.White;
            m_animatedSprite.Position = m_originalSpritePosition;
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

    protected override void UpdateAnimation(Vector2 p_direction)
    {
        if (m_animatedSprite == null) return;

        bool isAttacking = m_attackController != null && m_attackController.IsAttacking;

        if (isAttacking)
        {
            // Attack animation is handled by OnAttackStarted via signals
            return;
        }

        if (Velocity.LengthSquared() > 0)
        {
            if (m_animatedSprite.Animation != m_animMoving)
            {
                m_animatedSprite.Play(m_animMoving);
            }
        }
        else
        {
            StringName targetIdleAnim = m_animIdle;

            // If we are in the Shielded phase (under 66% HP but not enraged), play Shielded idle animation
            if (m_bossController != null && m_bossController.CurrentPhase == IslandSurvivor.Logic.Entities.BossPhase.Shielded)
            {
                targetIdleAnim = new StringName("IdleShielded");
            }

            if (m_animatedSprite.Animation != targetIdleAnim)
            {
                m_animatedSprite.Play(targetIdleAnim);
            }
        }
    }

    protected override void OnAttackStarted()
    {
        string animName = m_bossController.CurrentPhase == IslandSurvivor.Logic.Entities.BossPhase.Enraged ? EnragedAttackAnimationName : NormalAttackAnimationName;
        PlayAttackAnimation(animName);
    }
}
