namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class FleeState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float FleeDuration { get; set; } = 3.0f;
    [Export] public string TargetStateOnStop { get; set; } = "IdleState";

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Flee";
    [Export] public string FallbackAnimationName { get; set; } = "Error";

    private AnimationPlayer m_animationPlayer = null!;
    private float m_timer;
    private Vector2 m_fleeDirection;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
    }

    public override void Enter()
    {
        m_timer = FleeDuration;

        if (m_animationPlayer != null)
        {
            if (m_animationPlayer.HasAnimation(AnimationName))
            {
                m_animationPlayer.Play(AnimationName);
            }
            else if (m_animationPlayer.HasAnimation(FallbackAnimationName))
            {
                GD.PushWarning($"[FleeState] Animation '{AnimationName}' not found. Playing '{FallbackAnimationName}'.");
                m_animationPlayer.Play(FallbackAnimationName);
            }
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.Passive.PassiveNpcBase passive && passive.GetNodeOrNull<Node2D>("LastAttacker") is Node2D attacker)
        {
            m_fleeDirection = (NpcContext.GlobalPosition - attacker.GlobalPosition).Normalized();
        }
        else
        {
            float angle = (float)GD.RandRange(0, Mathf.Pi * 2);
            m_fleeDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    public override void PhysicsUpdate(double p_delta)
    {
        m_timer -= (float)p_delta;

        if (m_timer <= 0)
        {
            TransitionTo(TargetStateOnStop);
            return;
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.Passive.PassiveNpcBase passive)
        {
            if (passive.MovementController != null)
            {
                passive.MovementController.Move(m_fleeDirection, passive.FleeSpeed);
            }
            else
            {
                passive.Velocity = m_fleeDirection * passive.FleeSpeed;
                passive.MoveAndSlide();
            }
        }
    }
}
