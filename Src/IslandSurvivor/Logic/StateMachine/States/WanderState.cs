namespace IslandSurvivor.Logic.StateMachine.States;

using System;
using Godot;

[GlobalClass]
public partial class WanderState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float WanderRadius { get; set; } = 100.0f;
    [Export] public float WanderSpeed { get; set; } = 50.0f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Moving";
    [Export] public string FallbackAnimationName { get; set; } = "Error";

    private Vector2 m_spawnPosition;
    private Vector2 m_targetPosition;
    private AnimationPlayer? m_animationPlayer;
    private Sprite2D? m_sprite;
    private bool m_hasCompleted = false;

    private readonly Random m_random = new Random();

    public override bool IsActionState => false;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");

        if (NpcContext != null)
        {
            m_spawnPosition = NpcContext.GlobalPosition;
        }
    }

    public override void Enter()
    {
        base.Enter();
        m_hasCompleted = false;

        if (m_animationPlayer != null)
        {
            if (m_animationPlayer.HasAnimation(AnimationName))
            {
                m_animationPlayer.Play(AnimationName);
            }
            else if (m_animationPlayer.HasAnimation(FallbackAnimationName))
            {
                GD.PushWarning($"[WanderState] Animation '{AnimationName}' not found. Playing '{FallbackAnimationName}'.");
                m_animationPlayer.Play(FallbackAnimationName);
            }
        }

        // Pick a random target position within WanderRadius from the spawn position
        float angle = (float)(m_random.NextDouble() * Math.PI * 2);
        float distance = (float)(m_random.NextDouble() * WanderRadius);
        m_targetPosition = m_spawnPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
    }

    public override void PhysicsUpdate(double p_delta)
    {
        if (m_hasCompleted || NpcContext == null) return;

        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggressiveNpc)
        {
            if (aggressiveNpc.HasTargetAndLineOfSight())
            {
                m_hasCompleted = true;
                aggressiveNpc.Velocity = Vector2.Zero;
                CompleteState(StateExitReason.TargetDetected);
                return;
            }
        }

        float distanceToTargetSquared = NpcContext.GlobalPosition.DistanceSquaredTo(m_targetPosition);
        float completionThreshold = 10.0f; // Distance threshold to consider target reached

        if (distanceToTargetSquared <= completionThreshold * completionThreshold)
        {
            m_hasCompleted = true;
            if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npcBaseComplete)
            {
                npcBaseComplete.Velocity = Vector2.Zero;
            }
            CompleteState(StateExitReason.Finished);
            return;
        }

        Vector2 direction = (m_targetPosition - NpcContext.GlobalPosition).Normalized();

        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npcBase)
        {
            if (npcBase.MovementController != null)
            {
                npcBase.MovementController.Move(direction, WanderSpeed);

                if (m_sprite != null && direction.X != 0)
                {
                    m_sprite.FlipH = direction.X < 0;
                }
            }
            else
            {
                npcBase.Velocity = direction * WanderSpeed;
                npcBase.MoveAndSlide();

                if (m_sprite != null && direction.X != 0)
                {
                    m_sprite.FlipH = direction.X < 0;
                }
            }
        }

        // Failsafe: if character is completely stuck (not moving), we should probably finish wander
        if (NpcContext.GetSlideCollisionCount() > 0)
        {
            m_hasCompleted = true;
            if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npcBaseCollision)
            {
                npcBaseCollision.Velocity = Vector2.Zero;
            }
            CompleteState(StateExitReason.Finished);
        }
    }
}
