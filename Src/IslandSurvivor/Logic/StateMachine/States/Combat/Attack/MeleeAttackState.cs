namespace IslandSurvivor.Logic.StateMachine;

using Godot;

[GlobalClass]
public partial class MeleeAttackState : AttackState
{
    [Export] public string AttackAnimationName { get; set; } = "Attack";
    [Export] public float AttackRange { get; set; } = 60.0f;
    public override void Enter()
    {
        base.Enter();

        if (m_hasCompleted) return;

        var (animSuffix, direction) = DetermineDirectionAndAnimation();
        string fullAnimName = $"{AttackAnimationName}{animSuffix}";

        // Check if the base AttackAnimationName already has the suffix to prevent double appending if configured wrongly
        if (AttackAnimationName.EndsWith("_Side") || AttackAnimationName.EndsWith("_Up") || AttackAnimationName.EndsWith("_Down"))
        {
            fullAnimName = AttackAnimationName;
        }
        else if (m_animationPlayer != null && !m_animationPlayer.HasAnimation(fullAnimName))
        {
            // If the suffixed animation doesn't exist, fallback to the base name
            fullAnimName = AttackAnimationName;
        }

        m_attackController.SetAttackAnimation(fullAnimName);
        m_attackController.TryAttack(direction);
    }
}
