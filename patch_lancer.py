import re

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Lancer/Lancer.cs', 'r') as f:
    content = f.read()

replacement = """    [Export] public float DashSpeedMultiplier { get; set; } = 3.0f;
    [Export] public float DashThreshold { get; set; } = 150.0f;

    protected override Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        if (distanceSquared <= attackRangeSquared)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.AttackStateName;
        }
        if (distanceSquared < DashThreshold * DashThreshold)
        {
            return IslandSurvivor.Logic.StateMachine.StateConstants.ChaseStateName;
        }
        return new Godot.StringName("LancerRepositionState");
    }"""

content = re.sub(r'    \[Export\] public float DashSpeedMultiplier \{ get; set; \} = 3\.0f;\n    \[Export\] public float DashThreshold \{ get; set; \} = 150\.0f;', replacement, content)

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Lancer/Lancer.cs', 'w') as f:
    f.write(content)
