namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class AttackState : State
{
    [Export] public string AttackAnimationName { get; set; } = "Attack";

    private IslandSurvivor.Nodes.Combat.AttackController m_attackController = null!;
    private Sprite2D m_sprite = null!;
    private bool m_hasCompleted = false;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
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

        if (m_attackController != null && m_attackController.CanAttack)
        {
            string animSuffix = "_Side";
            string direction = "Right";

            if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
            {
                var target = aggNpc.GetTarget();
                if (target != null)
                {
                    Vector2 toTarget = aggNpc.LockedDirection != Vector2.Zero
                        ? aggNpc.LockedDirection
                        : (target.GlobalPosition - NpcContext.GlobalPosition).Normalized();

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
                }
            }
            else
            {
                // Fallback for non-aggressive NPCs or missing target
                if (m_sprite != null)
                {
                    direction = m_sprite.FlipH ? "Left" : "Right";
                }
            }

            string fullAnimName = $"{AttackAnimationName}{animSuffix}";

            // Check if the base AttackAnimationName already has the suffix to prevent double appending if configured wrongly
            if (AttackAnimationName.EndsWith("_Side") || AttackAnimationName.EndsWith("_Up") || AttackAnimationName.EndsWith("_Down"))
            {
                fullAnimName = AttackAnimationName;
            }

            m_attackController.SetAttackAnimation(fullAnimName);
            m_attackController.TryAttack(direction);
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

    public override void Update(double p_delta)
    {
        if (m_hasCompleted) return;

        if (m_attackController != null)
        {
            if (!m_attackController.IsAttacking)
            {
                m_hasCompleted = true;
                CompleteState(StateExitReason.Finished);
            }
        }
        else
        {
            m_hasCompleted = true;
            CompleteState(StateExitReason.Finished);
        }
    }
}
