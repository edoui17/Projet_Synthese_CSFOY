namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class ChaseState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float ChaseSpeed { get; set; } = 120.0f;
    [Export] public float LoseInterestRange { get; set; } = 100.0f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Moving";
    [Export] public string FallbackAnimationName { get; set; } = "Error";

    private AnimationPlayer? m_animationPlayer;
    private Sprite2D? m_sprite;
    private IslandSurvivor.Nodes.AttackController? m_attackController;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
        m_attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.AttackController>("AttackController");

        if (NpcContext != null)
        {
            Area2D? detectionArea = NpcContext.GetNodeOrNull<Area2D>("DetectionArea");
            if (detectionArea != null)
            {
                var collisionShape = detectionArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
                if (collisionShape != null && collisionShape.Shape is CircleShape2D circleShape)
                {
                    float detectionRadius = circleShape.Radius;
                    if (LoseInterestRange <= detectionRadius)
                    {
                        GD.PushWarning($"[{NpcContext.Name}] LoseInterestRange ({LoseInterestRange}) is too small compared to DetectionRadius ({detectionRadius})! Auto-adjusting to prevent logic loops.");
                        LoseInterestRange = detectionRadius * 2.0f;
                    }
                }
            }
        }
    }

    public override void Enter()
    {
        base.Enter();
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
        else if (m_sprite != null)
        {
            // Animation playing is handled by State/AnimationPlayer
        }
    }

    public override void PhysicsUpdate(double p_delta)
    {
        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggressiveNpc)
        {
            if (!aggressiveNpc.HasTargetAndLineOfSight())
            {
                aggressiveNpc.Velocity = Vector2.Zero;
                CompleteState(StateExitReason.TargetLost);
                return;
            }

            var target = aggressiveNpc.GetTarget();
            if (target != null)
            {
                float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);

                if (distSquared > LoseInterestRange * LoseInterestRange)
                {
                    aggressiveNpc.Velocity = Vector2.Zero;
                    CompleteState(StateExitReason.TargetLost);
                }
                else
                {
                    bool isRanged = NpcContext is IslandSurvivor.Scenes.NPC.RangedAggressiveNpcBase;
                    bool inAttackRange = false;

                    if (isRanged)
                    {
                        inAttackRange = distSquared <= ((IslandSurvivor.Scenes.NPC.AggressiveNpcBase)NpcContext).MaxAttackRange * ((IslandSurvivor.Scenes.NPC.AggressiveNpcBase)NpcContext).MaxAttackRange;
                    }
                    else
                    {
                        inAttackRange = distSquared <= ((IslandSurvivor.Scenes.NPC.AggressiveNpcBase)NpcContext).AttackRange * ((IslandSurvivor.Scenes.NPC.AggressiveNpcBase)NpcContext).AttackRange;
                    }

                    if (inAttackRange)
                    {
                        aggressiveNpc.Velocity = Vector2.Zero;

                        // Cached to prevent GetNode allocations in hot path
                        if (m_attackController != null && m_attackController.CanAttack)
                        {
                            CompleteState(StateExitReason.TargetReached);
                        }
                        else
                        {
                            if (m_animationPlayer != null && m_animationPlayer.HasAnimation("Idle"))
                            {
                                m_animationPlayer.Play("Idle");
                            }
                        }
                    }
                    else
                    {
                        if (m_animationPlayer != null && m_animationPlayer.HasAnimation(AnimationName))
                        {
                            m_animationPlayer.Play(AnimationName);
                        }

                        Vector2 direction = (target.GlobalPosition - NpcContext.GlobalPosition).Normalized();

                        if (aggressiveNpc.MovementController != null)
                        {
                            aggressiveNpc.MovementController.Move(direction, ChaseSpeed);

                            if (m_sprite != null && direction.X != 0)
                            {
                                m_sprite.FlipH = direction.X < 0;
                            }
                        }
                        else
                        {
                            aggressiveNpc.Velocity = direction * ChaseSpeed;
                            aggressiveNpc.MoveAndSlide();

                            if (m_sprite != null && direction.X != 0)
                            {
                                m_sprite.FlipH = direction.X < 0;
                            }
                        }
                    }
                }
            }
        }
    }
}
