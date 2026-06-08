namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class RecoveryState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float RecoveryDuration { get; set; } = 1.0f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Idle";
    [Export] public string FallbackAnimationName { get; set; } = "Idle";

    public override bool IsActionState => true;

    private AnimationPlayer m_animationPlayer = null!;
    private float m_timer;
    private bool m_hasCompleted = false;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
    }

    public override void Enter()
    {
        base.Enter();
        m_timer = RecoveryDuration;
        m_hasCompleted = false;

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

        if (m_timer <= 0 && !m_hasCompleted)
        {
            if (StateMachine.TryGetState<GuardingState>(out var guardState))
            {
                if (Godot.GD.Randf() <= guardState.GuardChance)
                {
                    m_hasCompleted = true;
                    StateMachine.ForceTransition(guardState.Name);
                    return;
                }
            }

            CompleteState(StateExitReason.Finished);
        }
    }
}
