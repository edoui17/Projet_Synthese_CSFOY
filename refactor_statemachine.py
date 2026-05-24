import re

with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'r') as f:
    content = f.read()

def replacer(match):
    return """    private StringName GetCombatDecisionState()
    {
        if (NpcContext == null)
            return StateConstants.IdleStateName;

        var target = (NpcContext as IslandSurvivor.Scenes.NPC.Aggressive.AggressiveNpcBase)?.GetTarget();
        return NpcContext.GetDecisionState(target);
    }"""

content = re.sub(r'    private StringName GetCombatDecisionState\(\)\s*\{.*?(?=\n\n    public void ForceTransition)', replacer, content, flags=re.DOTALL)

with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'w') as f:
    f.write(content)
