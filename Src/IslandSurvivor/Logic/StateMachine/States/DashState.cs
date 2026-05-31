namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class DashState : State
{
    [ExportGroup("State Configuration")]
    [Export] public float DashSpeed { get; set; } = 400.0f;
    [Export] public float DashDuration { get; set; } = 0.5f;
    [Export] public float DashDamageMultiplier { get; set; } = 1.5f;

    [ExportGroup("State Animations")]
    [Export] public string AnimationName { get; set; } = "Dash";
    [Export] public string FallbackAnimationName { get; set; } = "Moving";

    public override bool IsActionState => true;

    private IslandSurvivor.Nodes.AttackController m_attackController = null!;
    private Sprite2D m_sprite = null!;
    private bool m_hasCompleted = false;
    private bool m_hasDealtDashDamage = false;
    private float m_timer;
    private Vector2 m_dashDirection = Vector2.Zero;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.AttackController>("AttackController");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Enter()
    {
        base.Enter();
        m_hasCompleted = false;
        m_hasDealtDashDamage = false;
        m_timer = DashDuration;

        if (NpcContext is IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc)
        {
            if (aggNpc.LockedDirection != Vector2.Zero)
            {
                m_dashDirection = aggNpc.LockedDirection;
            }
            else
            {
                var target = aggNpc.GetTarget();
                if (target != null)
                {
                    m_dashDirection = (target.GlobalPosition - NpcContext.GlobalPosition).Normalized();
                }
                else
                {
                    m_dashDirection = (m_sprite != null && m_sprite.FlipH) ? Vector2.Left : Vector2.Right;
                }
            }
        }

        if (m_attackController == null)
        {
            if (!m_hasCompleted)
            {
                m_hasCompleted = true;
                CompleteState(StateExitReason.Finished);
            }
            return;
        }

        // If the dash is triggered but CanAttack is false (e.g. cooldown), we reset the cooldown so the dash attack can still trigger
        if (!m_attackController.CanAttack)
        {
            // Reset cooldown properly via public API
            m_attackController.ResetCooldown();
        }

        // Apply damage multiplier for dash
        if (m_attackController.Stats != null)
        {
            m_attackController.Stats.BaseAttackValue *= DashDamageMultiplier;
        }

        string animSuffix = "_Side";
        string direction = "Right";

        if (System.Math.Abs(m_dashDirection.Y) > System.Math.Abs(m_dashDirection.X))
        {
            if (m_dashDirection.Y < 0)
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
            direction = m_dashDirection.X < 0 ? "Left" : "Right";
            if (m_sprite != null)
            {
                m_sprite.FlipH = m_dashDirection.X < 0;
            }
        }

        string fullAnimName = $"{AnimationName}{animSuffix}";

        if (AnimationName.EndsWith("_Side") || AnimationName.EndsWith("_Up") || AnimationName.EndsWith("_Down"))
        {
            fullAnimName = AnimationName;
        }

        m_attackController.SetAttackAnimation(fullAnimName);
        m_attackController.TryAttack(direction);
    }

    public override void Exit()
    {
        base.Exit();

        // Reset damage multiplier
        if (m_attackController != null && m_attackController.Stats != null)
        {
            m_attackController.Stats.BaseAttackValue /= DashDamageMultiplier;
        }

        if (NpcContext.HasMethod("DisableAllDashHitboxes"))
        {
            NpcContext.Call("DisableAllDashHitboxes");
        }
    }

    public override void PhysicsUpdate(double p_delta)
    {
        if (m_hasCompleted) return;

        m_timer -= (float)p_delta;

        if (m_timer <= 0)
        {
            if (m_attackController != null && m_attackController.IsAttacking)
            {
                m_attackController.CancelAttack();
            }
            m_hasCompleted = true;
            CompleteState(StateExitReason.Finished);
            return;
        }

        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
        {
            if (npc.MovementController != null)
            {
                npc.MovementController.Move(m_dashDirection, DashSpeed);
            }
            else
            {
                npc.Velocity = m_dashDirection * DashSpeed;
                npc.MoveAndSlide();
            }

            int collisionCount = npc.GetSlideCollisionCount();
            if (collisionCount == 0) return;

            HandleCollisions(npc, collisionCount);
        }
        m_hasDealtDashDamage = true;
    }

    private void HandleCollisions(IslandSurvivor.Scenes.NPC.NpcBase p_npc, int p_collisionCount)
    {
        for (int i = 0; i < p_collisionCount; i++)
        {
            KinematicCollision2D collision = p_npc.GetSlideCollision(i);
            var collider = collision.GetCollider();

            if (collider is StaticBody2D or TileMapLayer)
            {
                HandleDashInterruption();
                return;
            }

            if (collider is Node targetNode && targetNode.IsInGroup("Player"))
            {
                DealDashDamage(targetNode);
                HandleDashInterruption();
                return;
            }
        }
    }

    private void DealDashDamage(Node p_targetNode)
    {
        if (m_hasDealtDashDamage) return;

        if (NpcContext is not IslandSurvivor.Scenes.NPC.AggressiveNpcBase aggNpc || aggNpc.Stats == null) return;

        float baseDamage = aggNpc.Stats.BaseAttackValue;
        int finalDamage = Mathf.RoundToInt(baseDamage * DashDamageMultiplier);

        if (p_targetNode is Core.Interfaces.IDamageable damageable)
        {
            damageable.TakeDamage(finalDamage, NpcContext);
        }
        else if (p_targetNode.HasMethod("TakeDamage"))
        {
            p_targetNode.Call("TakeDamage", finalDamage, NpcContext);
        }

        m_hasDealtDashDamage = true;
    }

    private void HandleDashInterruption()
    {
        if (m_attackController != null && m_attackController.IsAttacking)
        {
            m_attackController.CancelAttack();
        }
        m_hasCompleted = true;
        CompleteState(StateExitReason.CollisionDetected);
    }
}
