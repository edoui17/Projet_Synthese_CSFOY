namespace IslandSurvivor.Logic.StateMachine;

using Godot;

public static class StateConstants
{
    // String constants for use in switch/case statements
    public const string IdleState = "IdleState";
    public const string ChaseState = "ChaseState";
    public const string AttackState = "AttackState";
    public const string WindUpState = "WindUpState";
    public const string DashState = "DashState";
    public const string RecoveryState = "RecoveryState";
    public const string FleeState = "FleeState";
    public const string RepositionState = "RepositionState";
    public const string GuardState = "GuardState";
    public const string DeathState = "DeathState";

    // StringName constants for performance and assignments
    public static readonly StringName IdleStateName = new StringName(IdleState);
    public static readonly StringName ChaseStateName = new StringName(ChaseState);
    public static readonly StringName AttackStateName = new StringName(AttackState);
    public static readonly StringName WindUpStateName = new StringName(WindUpState);
    public static readonly StringName DashStateName = new StringName(DashState);
    public static readonly StringName RecoveryStateName = new StringName(RecoveryState);
    public static readonly StringName FleeStateName = new StringName(FleeState);
    public static readonly StringName RepositionStateName = new StringName(RepositionState);
    public static readonly StringName GuardStateName = new StringName(GuardState);
    public static readonly StringName DeathStateName = new StringName(DeathState);
}
