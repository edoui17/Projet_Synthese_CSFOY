namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class RangedAttackState : State
{
    [Export] public string AttackAnimationName { get; set; } = "Attack";

    public override bool IsActionState => true;

    private IslandSurvivor.Nodes.Shooter m_shooter = null!;
    private IslandSurvivor.Nodes.AttackController m_attackController = null!;
    private Sprite2D m_sprite = null!;
    private bool m_hasCompleted = false;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_shooter = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Shooter>("Shooter");
        m_attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.AttackController>("AttackController");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Enter()
    {
        base.Enter();
        m_hasCompleted = false;

        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
        {
            npc.Velocity = Vector2.Zero;
        }

        if (m_shooter == null || !m_shooter.CanShoot || m_attackController == null || !m_attackController.CanAttack)
        {
            if (!m_hasCompleted)
            {
                m_hasCompleted = true;
                CompleteState(StateExitReason.Finished);
            }
            return;
        }

        var (animSuffix, direction) = DetermineDirectionAndAnimation();
        string fullAnimName = $"{AttackAnimationName}{animSuffix}";

        if (AttackAnimationName.EndsWith("_Side") || AttackAnimationName.EndsWith("_Up") || AttackAnimationName.EndsWith("_Down"))
        {
            fullAnimName = AttackAnimationName;
        }

        var callable = new Callable(this, nameof(OnAttackActionTriggered));
        if (!m_attackController.IsConnected(IslandSurvivor.Nodes.AttackController.SignalName.AttackActionTriggered, callable))
        {
            m_attackController.Connect(IslandSurvivor.Nodes.AttackController.SignalName.AttackActionTriggered, callable);
        }

        m_attackController.SetAttackAnimation(fullAnimName);
        m_attackController.TryAttack(direction);
    }

    private (string AnimSuffix, string Direction) DetermineDirectionAndAnimation()
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
        if (m_attackController != null)
        {
            var callable = new Callable(this, nameof(OnAttackActionTriggered));
            if (m_attackController.IsConnected(IslandSurvivor.Nodes.AttackController.SignalName.AttackActionTriggered, callable))
            {
                m_attackController.Disconnect(IslandSurvivor.Nodes.AttackController.SignalName.AttackActionTriggered, callable);
            }
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

        if (!m_attackController.IsAttacking)
        {
            m_hasCompleted = true;
            Callable.From(() => CompleteState(StateExitReason.Finished)).CallDeferred();
        }
    }

    private void OnAttackActionTriggered()
    {
        ExecuteShoot();
    }

    public void ExecuteShoot()
    {
        if (m_shooter != null)
        {
            m_shooter.Shoot();
        }
    }
}
