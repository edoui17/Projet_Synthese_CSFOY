namespace IslandSurvivor.Logic.StateMachine;

public enum StateExitReason
{
    Finished,
    TargetReached,
    TargetLost,
    TargetDetected,
    CooldownFinished,
    Interrupted,
    CollisionDetected
}
