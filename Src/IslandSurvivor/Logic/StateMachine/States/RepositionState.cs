namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class RepositionState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float RepositionDuration { get; set; } = 1.5f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Moving";
    [Export] public string FallbackAnimationName { get; set; } = "Error";

    private AnimationPlayer? m_animationPlayer;
    private Sprite2D? m_sprite;
    private float m_timer;
    private Vector2 m_repositionDirection;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Enter()
    {
        base.Enter();
        m_timer = RepositionDuration;

        if (m_animationPlayer != null)
        {
            if (m_animationPlayer.HasAnimation(AnimationName))
            {
                m_animationPlayer.Play(AnimationName);
            }
            else if (m_animationPlayer.HasAnimation(FallbackAnimationName))
            {
                GD.PushWarning($"[RepositionState] Animation '{AnimationName}' not found. Playing '{FallbackAnimationName}'.");
                m_animationPlayer.Play(FallbackAnimationName);
            }
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggressiveNpc)
        {
            var target = aggressiveNpc.GetTarget();
            if (target != null)
            {
                m_repositionDirection = (NpcContext.GlobalPosition - target.GlobalPosition).Normalized();
            }
            else
            {
                float angle = (float)GD.RandRange(0, Mathf.Pi * 2);
                m_repositionDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
        }
    }

    public override void PhysicsUpdate(double p_delta)
    {
        m_timer -= (float)p_delta;

        if (m_timer <= 0)
        {
            NpcContext.Velocity = Vector2.Zero;
            CompleteState(StateExitReason.Finished);
            return;
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggressiveNpc)
        {
            float speed = aggressiveNpc.ChaseSpeed;
            if (aggressiveNpc.MovementController != null)
            {
                aggressiveNpc.MovementController.Move(m_repositionDirection, speed);
                if (m_sprite != null && m_repositionDirection.X != 0)
                {
                    m_sprite.FlipH = m_repositionDirection.X < 0;
                }
            }
            else
            {
                aggressiveNpc.Velocity = m_repositionDirection * speed;
                aggressiveNpc.MoveAndSlide();
                if (m_sprite != null && m_repositionDirection.X != 0)
                {
                    m_sprite.FlipH = m_repositionDirection.X < 0;
                }
            }

            var target = aggressiveNpc.GetTarget();
            if (target != null)
            {
                float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
                // Stop repositioning early if we are comfortably out of the minimum range (e.g., halfway to max range)
                float idealRangeSquared = (StateMachine.MinAttackRange + (StateMachine.MaxAttackRange - StateMachine.MinAttackRange) / 2.0f);
                idealRangeSquared *= idealRangeSquared;

                if (distSquared >= idealRangeSquared)
                {
                    NpcContext.Velocity = Vector2.Zero;
                    CompleteState(StateExitReason.Finished);
                }
            }
        }
    }
}
