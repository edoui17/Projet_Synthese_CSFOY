namespace IslandSurvivor.Logic.StateMachine;

using Godot;

public partial class AttackState : State
{

    public override bool IsActionState => true;

    protected IslandSurvivor.Nodes.AttackController m_attackController = null!;
    protected Sprite2D m_sprite = null!;
    protected AnimationPlayer m_animationPlayer = null!;
    protected bool m_hasCompleted = false;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.AttackController>("AttackController");
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

        if (m_attackController == null || !m_attackController.CanAttack)
        {
            if (!m_hasCompleted)
            {
                m_hasCompleted = true;
                CompleteState(StateExitReason.Finished);
            }
        }
    }

    protected (string AnimSuffix, string Direction) DetermineDirectionAndAnimation()
    {
        string animSuffix = "_Side";
        string direction = "Right";

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc)
        {
            var target = aggNpc.GetTarget();
            if (GodotObject.IsInstanceValid(target))
            {
                // Lock direction toward target immediately to prevent jitter during attack animation
                Vector2 safeDir = NpcContext.GlobalPosition.DirectionTo(target.GlobalPosition);
                if (safeDir != Vector2.Zero)
                {
                    aggNpc.LockedDirection = safeDir;
                }
                Vector2 toTarget = aggNpc.LockedDirection;

                if (System.Math.Abs(toTarget.Y) > System.Math.Abs(toTarget.X))
                {
                    if (toTarget.Y < 0)
                    {
                        animSuffix = "_Up";
                        direction = "Up";
                    }
                    else
                    {
                        animSuffix = "_Down";
                        direction = "Down";
                    }
                }
                else
                {
                    animSuffix = "_Side";
                    direction = toTarget.X < 0 ? "Left" : "Right";
                    if (m_sprite != null)
                    {
                        m_sprite.FlipH = toTarget.X < 0;
                    }
                }

                return (animSuffix, direction);
            }
        }

        // Fallback for non-aggressive NPCs or missing target
        if (m_sprite != null)
        {
            direction = m_sprite.FlipH ? "Left" : "Right";
        }

        return (animSuffix, direction);
    }

    public override void Exit()
    {
        base.Exit();

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc)
        {
            aggNpc.LockedDirection = Vector2.Zero;
        }

        if (m_attackController != null && m_attackController.IsAttacking)
        {
            m_attackController.CancelAttack();
        }
    }

    public override void Update(double p_delta)
    {
        if (m_hasCompleted) return;

        if (m_attackController == null)
        {
            m_hasCompleted = true;
            CompleteState(StateExitReason.Finished);
            return;
        }

        // Safety check: complete if controller is NOT attacking
        if (!m_attackController.IsAttacking)
        {
            m_hasCompleted = true;
            Callable.From(() => CompleteState(StateExitReason.Finished)).CallDeferred();
        }
    }
}
