namespace IslandSurvivor.Logic.StateMachine;

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
    private int m_collisionCount;
    private float m_lastCollisionTime;

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
        m_collisionCount = 0;
        m_lastCollisionTime = 0;

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

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggressiveNpc)
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

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggressiveNpc)
        {
            float speed = aggressiveNpc.ChaseSpeed;
            if (aggressiveNpc.MovementController != null)
            {
                aggressiveNpc.MovementController.Move(m_repositionDirection, speed);
            }
            else
            {
                aggressiveNpc.Velocity = m_repositionDirection * speed;
                aggressiveNpc.MoveAndSlide();
            }

            if (m_sprite != null && m_repositionDirection.X != 0)
            {
                m_sprite.FlipH = m_repositionDirection.X < 0;
            }

            // Handle wall collisions by sliding / picking a perpendicular direction
            if (NpcContext.GetSlideCollisionCount() > 0)
            {
                // Debounce collisions slightly so we don't spam direction changes every frame
                if (m_timer < m_lastCollisionTime - 0.1f || m_lastCollisionTime == 0)
                {
                    m_collisionCount++;
                    m_lastCollisionTime = m_timer;

                    if (m_collisionCount >= 3)
                    {
                        // Stuck in a corner or heavily hitting walls, stop repositioning
                        NpcContext.Velocity = Vector2.Zero;
                        CompleteState(StateExitReason.Finished);
                        return;
                    }

                    // Pick a perpendicular direction. Rotate by 90 degrees (Pi/2)
                    // Randomly choose left or right to avoid getting stuck in loops
                    float rotation = GD.Randf() > 0.5f ? Mathf.Pi / 2.0f : -Mathf.Pi / 2.0f;
                    m_repositionDirection = m_repositionDirection.Rotated(rotation).Normalized();
                }
            }

            var target = aggressiveNpc.GetTarget();
            if (target != null)
            {
                float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
                // Stop repositioning early if we are comfortably out of the minimum range (e.g., halfway to max range)
                float idealRangeSquared = aggressiveNpc.MinAttackRange + (aggressiveNpc.MaxAttackRange - aggressiveNpc.MinAttackRange) / 2.0f;
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
