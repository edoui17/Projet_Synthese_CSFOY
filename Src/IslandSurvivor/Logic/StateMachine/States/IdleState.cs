namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class IdleState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float WaitTime { get; set; } = 2.0f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Idle";
    [Export] public string FallbackAnimationName { get; set; } = "Error";

    private float m_timer;
    private AnimationPlayer? m_animationPlayer;
    private Sprite2D? m_sprite;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Enter()
    {
        base.Enter();
        m_timer = WaitTime;

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
        m_timer -= (float)p_delta;

        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggressiveNpc)
        {
            if (aggressiveNpc.HasTargetAndLineOfSight())
            {
                CompleteState(StateExitReason.TargetDetected);
                return;
            }
        }

        if (m_timer <= 0)
        {
            m_timer = WaitTime;
        }
    }
}
