namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class DashState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float DashSpeed { get; set; } = 400.0f;
    [Export] public float DashDuration { get; set; } = 0.5f;
    [Export] public string TargetStateOnComplete { get; set; } = "RecoveryState";
    [Export] public string TargetStateOnCollision { get; set; } = "RecoveryState";

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Dash";
    [Export] public string FallbackAnimationName { get; set; } = "Moving";

    private AnimationPlayer m_animationPlayer = null!;
    private float m_timer;
    private Vector2 m_dashDirection = Vector2.Zero;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
    }

    public override void Enter()
    {
        m_timer = DashDuration;

        if (m_animationPlayer != null)
        {
            if (m_animationPlayer.HasAnimation(AnimationName))
            {
                m_animationPlayer.Play(AnimationName);
            }
            else if (m_animationPlayer.HasAnimation(FallbackAnimationName))
            {
                m_animationPlayer.Play(FallbackAnimationName);
            }
        }

        // Try to get the locked direction from the WindUpState if it exists in the same StateMachine
        var windUpState = StateMachine.GetNodeOrNull<WindUpState>("WindUpState");
        if (windUpState != null && windUpState.LockedDirection != Vector2.Zero)
        {
            m_dashDirection = windUpState.LockedDirection;
        }
        else if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
        {
            // Fallback: calculate direction now if no WindUp state provided it
            var target = aggNpc.GetTarget();
            if (target != null)
            {
                m_dashDirection = (target.GlobalPosition - NpcContext.GlobalPosition).Normalized();
            }
            else
            {
                // Failsafe: dash forward based on facing direction
                var sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
                m_dashDirection = (sprite != null && sprite.FlipH) ? Vector2.Left : Vector2.Right;
            }
        }

        if (NpcContext.HasMethod("EnableDashHitbox"))
        {
            NpcContext.Call("EnableDashHitbox", m_dashDirection);
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (NpcContext.HasMethod("DisableAllDashHitboxes"))
        {
            NpcContext.Call("DisableAllDashHitboxes");
        }
    }

    public override void PhysicsUpdate(double p_delta)
    {
        m_timer -= (float)p_delta;

        if (m_timer <= 0)
        {
            TransitionTo(TargetStateOnComplete);
            return;
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
        {
            if (npc.MovementController != null)
            {
                // Dash overrides stats modifications
                npc.MovementController.Move(m_dashDirection, DashSpeed);
            }
            else
            {
                npc.Velocity = m_dashDirection * DashSpeed;
                npc.MoveAndSlide();
            }

            // If we hit a wall while dashing, transition early
            if (npc.GetSlideCollisionCount() > 0)
            {
                for (int i = 0; i < npc.GetSlideCollisionCount(); i++)
                {
                    KinematicCollision2D collision = npc.GetSlideCollision(i);
                    if (collision.GetCollider() is StaticBody2D or TileMapLayer)
                    {
                        TransitionTo(TargetStateOnCollision);
                        return;
                    }
                }
            }
        }
    }
}
