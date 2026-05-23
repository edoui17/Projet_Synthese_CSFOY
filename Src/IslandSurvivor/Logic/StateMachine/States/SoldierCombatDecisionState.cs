namespace IslandSurvivor.Logic.StateMachine.States;

using Godot;

[GlobalClass]
public partial class SoldierCombatDecisionState : State
{
    [Export] public string AttackStateName { get; set; } = "AttackState";
    [Export] public string GuardStateName { get; set; } = "GuardState";
    [Export] public float GuardChance { get; set; } = 0.5f; // 50% chance to guard if close and cooldown is up

    public override void Enter()
    {
        if (NpcContext is IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase aggNpc)
        {
            if (aggNpc.CanGuard)
            {
                // Random roll to decide between attack and guard
                float roll = (float)GD.Randf();
                if (roll <= GuardChance)
                {
                    TransitionTo(GuardStateName);
                    return;
                }
            }
        }

        // Default to attack
        TransitionTo(AttackStateName);
    }
}
