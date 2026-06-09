namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class ChaseState : MovementState
{
    [ExportGroup("State Configuration")]
    [Export] public float ChaseSpeed { get; set; } = 120.0f;
    [Export] public float LoseInterestRange { get; set; } = 100.0f;
    [Export] public float ChaseTimeout { get; set; } = 4.0f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Moving";

    private AnimationPlayer? m_animationPlayer;
    private Sprite2D? m_sprite;
    private IslandSurvivor.Nodes.AttackController? m_attackController;
    private float m_chaseTimer;
    private bool m_hasCompleted = false;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
        m_attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.AttackController>("AttackController");

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggressiveNpc)
        {
            if (LoseInterestRange <= aggressiveNpc.DetectionRadius)
            {
                GD.PushWarning($"[{NpcContext.Name}] LoseInterestRange ({LoseInterestRange}) is too small compared to DetectionRadius ({aggressiveNpc.DetectionRadius})! Auto-adjusting to prevent logic loops.");
                LoseInterestRange = aggressiveNpc.DetectionRadius * 2.0f;
            }
        }
    }

    public override void Enter()
    {
        base.Enter();
        m_chaseTimer = ChaseTimeout;
        m_hasCompleted = false;

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
        if (NpcContext is not IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggressiveNpc)
            return;

        if (m_hasCompleted) return;

        m_chaseTimer -= (float)p_delta;
        if (m_chaseTimer <= 0)
        {
            SetVelocity(Vector2.Zero);
            CompleteState(StateExitReason.Timeout);
            return;
        }

        var target = aggressiveNpc.GetTarget();
        if (target == null || !aggressiveNpc.CheckLineOfSight())
        {
            SetVelocity(Vector2.Zero);
            CompleteState(StateExitReason.TargetLost);
            return;
        }

        float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);

        if (distSquared > LoseInterestRange * LoseInterestRange)
        {
            SetVelocity(Vector2.Zero);
            CompleteState(StateExitReason.TargetLost);
            return;
        }

        if (StateMachine.TryGetState<DashState>(out var dashState))
        {
            if (distSquared <= dashState.DashThreshold * dashState.DashThreshold &&
                distSquared >= dashState.MinDashDistance * dashState.MinDashDistance &&
                dashState.CanDash())
            {
                if (StateMachine.HasState(StateConstants.RepositionStateName))
                {
                    StateMachine.ForceTransition(StateConstants.RepositionStateName);
                }
                else
                {
                    m_hasCompleted = true;
                    StateMachine.ForceTransition(StateConstants.DashStateName);
                }
                return;
            }
        }

        float minAttackRange = 80.0f;
        float attackRange = 0f;
        if (StateMachine.TryGetState<MeleeAttackState>(out var meleeState))
        {
            attackRange = meleeState.AttackRange;
        }
        if (StateMachine.TryGetState<RangedAttackState>(out var rangedState))
        {
            attackRange = Mathf.Max(attackRange, rangedState.MaxAttackRange);
        }
        if (StateMachine.TryGetState<MagicAttackState>(out var magicState))
        {
            attackRange = Mathf.Max(attackRange, magicState.MaxAttackRange);
        }
        bool isRanged = StateMachine.TryGetState<RangedAttackState>(out _) || StateMachine.TryGetState<MagicAttackState>(out _);

        bool isBoss = NpcContext is IslandSurvivor.Scenes.NPC.BossBase;

        if (distSquared <= attackRange * attackRange)
        {
            bool reachedTarget = false;

            if (isBoss)
            {
                if (distSquared <= minAttackRange * minAttackRange)
                {
                    reachedTarget = true; // In melee range
                }
                else
                {
                    // In ranged zone. Only stop chasing if we can shoot.
                    var shooter = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Shooter>("Shooter");
                    if (shooter != null && shooter.CanShoot)
                    {
                        reachedTarget = true;
                    }
                }
            }
            else if (isRanged)
            {
                if (distSquared <= minAttackRange * minAttackRange)
                {
                    reachedTarget = true;
                }
                else
                {
                    var shooter = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Shooter>("Shooter");
                    if (shooter != null && shooter.CanShoot)
                    {
                        reachedTarget = true;
                    }
                    else if (m_attackController != null && m_attackController.CanAttack)
                    {
                        reachedTarget = true;
                    }
                }
            }
            else
            {
                reachedTarget = true; // Melee
            }

            if (reachedTarget)
            {
                SetVelocity(Vector2.Zero);
                CompleteState(StateExitReason.TargetReached);
                return;
            }
        }

        if (m_animationPlayer != null && m_animationPlayer.HasAnimation(AnimationName))
        {
            m_animationPlayer.Play(AnimationName);
        }

        Vector2 direction = (target.GlobalPosition - NpcContext.GlobalPosition).Normalized();

        if (aggressiveNpc.MovementController != null)
        {
            aggressiveNpc.MovementController.Move(direction, ChaseSpeed);
        }
        else
        {
            SetVelocity(direction * ChaseSpeed);
        }

        base.PhysicsUpdate(p_delta);

        if (m_sprite != null && direction.X != 0)
        {
            m_sprite.FlipH = direction.X < 0;
        }
    }
}
