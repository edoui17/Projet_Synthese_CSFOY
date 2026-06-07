namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class RangedAttackState : AttackState
{
    private IslandSurvivor.Nodes.Shooter m_shooter = null!;

    public override void Initialize(StateMachine p_stateMachine, CharacterBody2D p_npcContext)
    {
        base.Initialize(p_stateMachine, p_npcContext);
        m_shooter = NpcContext.GetNodeOrNull<IslandSurvivor.Nodes.Shooter>("Shooter");
    }

    public override void Enter()
    {
        base.Enter();

        if (m_hasCompleted) return;

        if (m_shooter == null || !m_shooter.CanShoot)
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
        else if (m_animationPlayer != null && !m_animationPlayer.HasAnimation(fullAnimName))
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
