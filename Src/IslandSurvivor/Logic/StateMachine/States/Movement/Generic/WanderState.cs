namespace IslandSurvivor.Logic.StateMachine;

using System;
using Godot;

[GlobalClass]
public partial class WanderState : MovementState
{
    [ExportGroup("State Configuration")]
    [Export] public float WanderRadius { get; set; } = 100.0f;
    [Export] public float WanderSpeed { get; set; } = 50.0f;
    [Export] public float StuckWaitTime { get; set; } = 0.5f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Moving";

    private Vector2 m_spawnPosition;
    private bool m_isSpawnPositionSet = false;
    private Vector2 m_targetPosition;
    private AnimationPlayer? m_animationPlayer;
    private Sprite2D? m_sprite;
    private bool m_hasCompleted = false;
    private bool m_isStuck = false;
    private float m_stuckTimer = 0.0f;

    public override bool IsActionState => false;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    private void SetNewTargetPosition()
    {
        float angle = (float)(GD.Randf() * Math.PI * 2);
        float distance = (float)(GD.Randf() * WanderRadius);
        m_targetPosition = m_spawnPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
    }

    public override void Enter()
    {
        base.Enter();

        if (!m_isSpawnPositionSet && NpcContext != null)
        {
            m_spawnPosition = NpcContext.GlobalPosition;
            m_isSpawnPositionSet = true;
        }

        m_hasCompleted = false;
        m_isStuck = false;
        m_stuckTimer = 0.0f;

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

        SetNewTargetPosition();
    }

    public override void PhysicsUpdate(double p_delta)
    {
        if (m_hasCompleted || NpcContext == null) return;

        if (m_isStuck)
        {
            m_stuckTimer -= (float)p_delta;
            if (m_stuckTimer <= 0)
            {
                m_isStuck = false;
                SetNewTargetPosition();

                if (m_animationPlayer != null && m_animationPlayer.HasAnimation(AnimationName))
                {
                    m_animationPlayer.Play(AnimationName);
                }
            }
            return;
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggressiveNpc)
        {
            var target = aggressiveNpc.GetTarget();
            if (target != null)
            {
                float distSquared = NpcContext.GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
                if (distSquared <= aggressiveNpc.DetectionRadius * aggressiveNpc.DetectionRadius && aggressiveNpc.CheckLineOfSight())
                {
                    m_hasCompleted = true;
                    aggressiveNpc.Velocity = Vector2.Zero;
                    CompleteState(StateExitReason.TargetDetected);
                    return;
                }
            }
        }

        float distanceToTargetSquared = NpcContext.GlobalPosition.DistanceSquaredTo(m_targetPosition);
        float completionThreshold = 10.0f; // Distance threshold to consider target reached

        if (distanceToTargetSquared <= completionThreshold * completionThreshold)
        {
            m_hasCompleted = true;
            SetVelocity(Vector2.Zero);
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
                SetVelocity(direction * WanderSpeed);

                if (m_sprite != null && direction.X != 0)
                {
                    m_sprite.FlipH = direction.X < 0;
                }
            }
        }

        // Failsafe: if character is completely stuck (not moving), we pause and pick a new direction
        if (NpcContext.GetSlideCollisionCount() > 0)
        {
            m_isStuck = true;
            m_stuckTimer = StuckWaitTime;

            SetVelocity(Vector2.Zero);
            if (m_animationPlayer != null && m_animationPlayer.HasAnimation("Idle"))
            {
                m_animationPlayer.Play("Idle");
            }
            else if (m_animationPlayer != null)
            {
                m_animationPlayer.Stop();
            }
        }
        base.PhysicsUpdate(p_delta);
    }
}
