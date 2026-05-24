namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class DeathState : State
{
    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Death";
    [Export] public string FallbackAnimationName { get; set; } = "Error";

    private AnimationPlayer m_animationPlayer = null!;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
    }

    public override void Enter()
    {
        base.Enter();
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
                GD.PushWarning($"[DeathState] Animation '{AnimationName}' not found. Playing '{FallbackAnimationName}'.");
                m_animationPlayer.Play(FallbackAnimationName);
            }
        }
    }
}
