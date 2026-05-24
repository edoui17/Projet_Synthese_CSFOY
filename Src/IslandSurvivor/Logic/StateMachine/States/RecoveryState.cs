namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class RecoveryState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float RecoveryDuration { get; set; } = 1.0f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Idle";
    [Export] public string FallbackAnimationName { get; set; } = "Idle";

    private AnimationPlayer m_animationPlayer = null!;
    private float m_timer;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
    }

    public override void Enter()
    {
        m_timer = RecoveryDuration;

        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
        {
            npc.Velocity = Vector2.Zero;
        }

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
    }

    public override void Update(double p_delta)
    {
        m_timer -= (float)p_delta;

        if (m_timer <= 0)
        {
            CompleteState(StateExitReason.Finished);
        }
    }
}
