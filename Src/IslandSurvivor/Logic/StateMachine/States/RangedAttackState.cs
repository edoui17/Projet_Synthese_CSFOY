namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class RangedAttackState : State
{
    [Export] public string AttackAnimationName { get; set; } = "Attack_Ranged";

    public override bool IsActionState => true;

    private IslandSurvivor.Nodes.Combat.Shooter m_shooter = null!;
    private Sprite2D m_sprite = null!;
    private AnimationPlayer m_animationPlayer = null!;
    private bool m_hasCompleted = false;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_shooter = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Combat.Shooter>("Shooter");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
        m_animationPlayer = NpcContext.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
    }

    public override void Enter()
    {
        base.Enter();
        m_hasCompleted = false;

        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
        {
            npc.Velocity = Vector2.Zero;
        }

        if (m_shooter != null && m_shooter.CanShoot)
        {
            if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
            {
                var target = aggNpc.GetTarget();
                if (GodotObject.IsInstanceValid(target))
                {
                    // Lock direction toward target
                    Vector2 safeDir = NpcContext.GlobalPosition.DirectionTo(target.GlobalPosition);
                    if (safeDir != Vector2.Zero)
                    {
                        aggNpc.LockedDirection = safeDir;
                    }
                    if (m_sprite != null)
                    {
                        m_sprite.FlipH = aggNpc.LockedDirection.X < 0;
                    }
                }
            }

            if (m_animationPlayer != null)
            {
                var callable = new Callable(this, nameof(OnAnimationFinished));
                if (m_animationPlayer.IsConnected(AnimationPlayer.SignalName.AnimationFinished, callable))
                {
                    m_animationPlayer.Disconnect(AnimationPlayer.SignalName.AnimationFinished, callable);
                }
                m_animationPlayer.Connect(AnimationPlayer.SignalName.AnimationFinished, callable);

                if (m_animationPlayer.HasAnimation(AttackAnimationName))
                {
                    m_animationPlayer.Play(AttackAnimationName);
                }
                else
                {
                    m_shooter.Shoot();
                    m_hasCompleted = true;
                    CompleteState(StateExitReason.Finished);
                }
            }
            else
            {
                m_shooter.Shoot();
                m_hasCompleted = true;
                CompleteState(StateExitReason.Finished);
            }
        }
        else
        {
            if (!m_hasCompleted)
            {
                m_hasCompleted = true;
                CompleteState(StateExitReason.Finished);
            }
        }
    }

    private void OnAnimationFinished(StringName p_animName)
    {
        if (p_animName == AttackAnimationName)
        {
            if (m_animationPlayer != null)
            {
                var callable = new Callable(this, nameof(OnAnimationFinished));
                if (m_animationPlayer.IsConnected(AnimationPlayer.SignalName.AnimationFinished, callable))
                {
                    m_animationPlayer.Disconnect(AnimationPlayer.SignalName.AnimationFinished, callable);
                }
            }
            m_hasCompleted = true;
            CompleteState(StateExitReason.Finished);
        }
    }

    public void ExecuteShoot()
    {
        if (m_shooter != null)
        {
            m_shooter.Shoot();
        }
    }
}
