namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class ChaseState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float ChaseSpeed { get; set; } = 120.0f;
    [Export] public float StopDistance { get; set; } = 40.0f;
    [Export] public string TargetStateOnStop { get; set; } = "AttackState";
    [Export] public string TargetStateOnLoseSight { get; set; } = "IdleState";

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Moving";
    [Export] public string FallbackAnimationName { get; set; } = "Error";

    private AnimationPlayer? m_animationPlayer;
    private AnimatedSprite2D? m_animatedSprite;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_animatedSprite = NpcContext.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void Enter()
    {
        if (m_animationPlayer != null)
        {
            if (m_animationPlayer.HasAnimation(AnimationName))
            {
                m_animationPlayer.Play(AnimationName);
            }
            else if (m_animationPlayer.HasAnimation(FallbackAnimationName))
            {
                GD.PushWarning($"[ChaseState] Animation '{AnimationName}' not found. Playing '{FallbackAnimationName}'.");
                m_animationPlayer.Play(FallbackAnimationName);
            }
        }
        else if (m_animatedSprite != null)
        {
            m_animatedSprite.Play(AnimationName);
        }
    }

    public override void PhysicsUpdate(double p_delta)
    {
        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggressiveNpc)
        {
            if (!aggressiveNpc.HasTargetAndLineOfSight())
            {
                aggressiveNpc.Velocity = Vector2.Zero;
                TransitionTo(TargetStateOnLoseSight);
                return;
            }

            var target = aggressiveNpc.GetTarget();
            if (target != null)
            {
                float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);

                if (distSquared <= StopDistance * StopDistance)
                {
                    aggressiveNpc.Velocity = Vector2.Zero;
                    TransitionTo(TargetStateOnStop);
                }
                else
                {
                    Vector2 direction = (target.GlobalPosition - NpcContext.GlobalPosition).Normalized();

                    if (aggressiveNpc.MovementController != null)
                    {
                        aggressiveNpc.MovementController.Move(direction, ChaseSpeed);
                    }
                    else
                    {
                        aggressiveNpc.Velocity = direction * ChaseSpeed;
                        aggressiveNpc.MoveAndSlide();

                        if (m_animatedSprite != null && direction.X != 0)
                        {
                            m_animatedSprite.FlipH = direction.X < 0;
                        }
                    }
                }
            }
        }
    }
}
