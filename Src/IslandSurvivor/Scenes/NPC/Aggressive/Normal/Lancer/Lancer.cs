namespace IslandSurvivor.Scenes.NPC;

using Godot;
using IslandSurvivor.Globals;
using IslandSurvivor.Logic;

public partial class Lancer : MeleeAggressiveNpcBase
{
    [Export] public float DashThreshold { get; set; } = 150.0f;


    protected override Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        if (m_stateMachine == null) return IslandSurvivor.Logic.StateMachine.StateConstants.IdleStateName;

        float dashThresholdSquared = DashThreshold * DashThreshold;

        // Outside Dash Threshold -> Chase
        if (distanceSquared > dashThresholdSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }

        // Inside Dash Threshold but outside Melee Range -> Dash or Chase
        if (distanceSquared > attackRangeSquared)
        {
            bool canAttack = m_attackController != null && m_attackController.CanAttack;

            if (canAttack && m_stateMachine.HasState(IslandSurvivor.Logic.StateMachine.StateConstants.RepositionStateName))
            {
                // We use LancerRepositionState which will transition to DashState
                return IslandSurvivor.Logic.StateMachine.StateConstants.RepositionStateName;
            }

            // If dash is on cooldown, chase to get into melee range
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }

        // Inside Melee Range -> Fallback to base evaluation (Melee / Guard / Idle)
        return base.GetCombatDecisionState(distanceSquared, attackRangeSquared);
    }

    public override void _Ready()
    {
        base._Ready();

        // Register dash hitboxes if needed, though DashState or AttackState usually handles this.
        // Actually, DashState handles the hitboxes dynamically.
        // The Lancer no longer uses LancerController and purely relies on its StateMachine.
    }
}
