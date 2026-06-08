namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class IdleState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float WaitTime { get; set; } = 2.0f;
    [Export] public float WanderCooldown { get; set; } = 3.0f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Idle";
    [Export] public string FallbackAnimationName { get; set; } = "Error";

    private float m_timer;
    private float m_wanderTimer;
    private AnimationPlayer? m_animationPlayer;
    private Sprite2D? m_sprite;
    private IslandSurvivor.Nodes.AttackController? m_attackController;
    private bool m_hasCompleted = false;

    public bool IsWanderCooldownElapsed => m_wanderTimer <= 0;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
        m_attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.AttackController>("AttackController");
    }

    public override void Enter()
    {
        base.Enter();
        m_timer = WaitTime;
        m_wanderTimer = WanderCooldown;
        m_hasCompleted = false;

        if (m_animationPlayer != null)
        {
            if (m_animationPlayer.HasAnimation(AnimationName))
            {
                m_animationPlayer.Play(AnimationName);
            }
            else if (m_animationPlayer.HasAnimation(FallbackAnimationName))
            {
                GD.PushWarning($"[IdleState] Animation '{AnimationName}' not found. Playing '{FallbackAnimationName}'.");
                m_animationPlayer.Play(FallbackAnimationName);
            }
        }
        else if (m_sprite != null)
        {
            // Animation playing is handled by State/AnimationPlayer
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
        {
            npc.Velocity = Vector2.Zero;
        }
    }

    public override void Update(double p_delta)
    {
        if (m_hasCompleted) return;

        m_timer -= (float)p_delta;

        m_wanderTimer -= (float)p_delta;

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggressiveNpc)
        {
            if (aggressiveNpc.HasTargetAndLineOfSight())
            {
                var target = aggressiveNpc.GetTarget();
                if (target != null)
                {
                    float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);

                    bool hasRanged = StateMachine.HasState(IslandSurvivor.Logic.StateMachine.StateConstants.RangedAttackStateName);
                    bool hasMagic = StateMachine.HasState(IslandSurvivor.Logic.StateMachine.StateConstants.MagicAttackStateName);
                    bool hasLongRangeAttacks = hasRanged || hasMagic;

                    float checkRange = hasLongRangeAttacks ? aggressiveNpc.MaxAttackRange : aggressiveNpc.MinAttackRange;

                    if (distSquared <= checkRange * checkRange)
                    {
                        // Cached to prevent GetNode allocations in hot path
                        bool isAttackCooldownReady = m_attackController != null && m_attackController.CanAttack;

                        // Let the StateMachine evaluate if it can guard or attack. We just notify that we are ready to transition.
                        if (isAttackCooldownReady || m_timer <= 0)
                        {
                            m_hasCompleted = true;
                            CompleteState(StateExitReason.CooldownFinished);
                            return;
                        }
                    }
                    else
                    {
                        m_hasCompleted = true;
                        CompleteState(StateExitReason.TargetDetected);
                        return;
                    }
                }
            }
        }

        if (m_timer <= 0)
        {
            if (IsWanderCooldownElapsed)
            {
                m_hasCompleted = true;
                CompleteState(StateExitReason.Finished);
                return;
            }
            m_timer = WaitTime;
        }
    }
}
