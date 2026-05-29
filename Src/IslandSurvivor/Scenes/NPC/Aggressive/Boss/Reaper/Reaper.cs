using Godot;
using System.Linq;

namespace IslandSurvivor.Scenes.NPC;

public partial class Reaper : BossBase
{
    [ExportGroup("Reaper Animations")]
    [Export] public string NormalAttackAnimationName { get; set; } = "MeleeAttackNormal";
    [Export] public string EnragedAttackAnimationName { get; set; } = "MeleeAttackEnraged";

    private bool m_isAnimationTestMode = false;
    private int m_currentAnimationIndex = 0;
    private string[] m_animationNames = System.Array.Empty<string>();

    private Tween? m_colorTween;
    private Tween? m_hoverTween;
    private Vector2 m_originalSpritePosition;


    protected override Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        float maxAttackRangeSquared = MaxAttackRange * MaxAttackRange;
        float minAttackRangeSquared = MinAttackRange * MinAttackRange;

        if (distanceSquared > maxAttackRangeSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }

        // Reaper specific logic: Melee when close, Ranged when far.
        // Assuming MinAttackRange acts as the Melee range.
        if (distanceSquared <= minAttackRangeSquared)
        {
            if (m_attackController != null && m_attackController.CanAttack)
            {
                var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
                if (windUp != null)
                {
                    windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.MeleeAttackStateName;
                }
                return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
            }
        }
        else
        {
            var shooter = GetNodeOrNull<IslandSurvivor.Nodes.Combat.Shooter>("Shooter");
            if (shooter != null && shooter.CanShoot)
            {
                var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
                if (windUp != null)
                {
                    windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.RangedAttackStateName;
                }
                return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
            }

            // If shooter is on cooldown, chase or idle?
            if (distanceSquared > minAttackRangeSquared)
            {
                return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
            }
        }

        return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;
    }

    public override void _Ready()
    {
        base._Ready();

        if (m_animationPlayer != null)
        {
            m_animationNames = m_animationPlayer.GetAnimationList().Select(x => x.ToString()).ToArray();
            if (m_sprite != null) m_originalSpritePosition = m_sprite.Position;
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
        if (m_sprite == null) return;

        string animName = m_animationNames[m_currentAnimationIndex];
        GD.Print($"[Reaper] Playing Animation: {animName}");

        ResetAnimationEffects();

        m_animationPlayer?.Play(animName);

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

    // Rely exclusively on state machine for Reaper AI.
}
