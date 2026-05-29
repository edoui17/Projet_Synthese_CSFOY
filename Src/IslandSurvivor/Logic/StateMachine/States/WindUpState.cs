namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class WindUpState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float WindUpDuration { get; set; } = 0.75f;
    [Export] public StringName NextStateAfterWindup { get; set; } = StateConstants.MeleeAttackStateName;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "WindUp";
    [Export] public string FallbackAnimationName { get; set; } = "Idle";

    public override bool IsActionState => true;

    private AnimationPlayer m_animationPlayer = null!;
    private float m_timer;
    private Sprite2D m_sprite = null!;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Enter()
    {
        base.Enter();
        m_timer = WindUpDuration;

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

        // Lock direction towards target immediately upon entering windup
        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
        {
            var target = aggNpc.GetTarget();
            if (target != null)
            {
                aggNpc.LockedDirection = (target.GlobalPosition - NpcContext.GlobalPosition).Normalized();
                if (m_sprite != null && aggNpc.LockedDirection.X != 0)
                {
                    m_sprite.FlipH = aggNpc.LockedDirection.X < 0;
                }
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
