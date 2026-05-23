namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class AttackState : State
{
    [Export] public string AttackAnimationName { get; set; } = "Attack";
    [Export] public string NextState { get; set; } = "ChaseState";

    private IslandSurvivor.Nodes.Combat.AttackController m_attackController = null!;
    private Sprite2D m_sprite = null!;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_attackController = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Combat.AttackController>("AttackController");
        m_sprite = NpcContext.GetNodeOrNull<Sprite2D>("Sprite2D");
    }

    public override void Enter()
    {
        if (NpcContext is IslandSurvivor.Scenes.NPC.NpcBase npc)
        {
            npc.Velocity = Vector2.Zero;
        }

        if (m_attackController != null && m_attackController.CanAttack)
        {
            if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
            {
                var target = aggNpc.GetTarget();
                if (target != null && m_sprite != null)
                {
                    m_sprite.FlipH = target.GlobalPosition.X < NpcContext.GlobalPosition.X;
                }
            }

            string direction = (m_sprite != null && m_sprite.FlipH) ? "Left" : "Right";
            m_attackController.SetAttackAnimation(AttackAnimationName);
            m_attackController.TryAttack(direction);
        }
        else
        {
            TransitionTo(NextState);
        }
    }

    public override void Update(double p_delta)
    {
        if (m_attackController != null)
        {
            if (!m_attackController.IsAttacking)
            {
                TransitionTo(NextState);
            }
        }
        else
        {
            TransitionTo(NextState);
        }
    }
}
