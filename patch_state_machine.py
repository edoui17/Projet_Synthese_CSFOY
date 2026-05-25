import re

file_path = 'Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs'
with open(file_path, 'r') as f:
    content = f.read()

# Replace AttackState with MeleeAttackState
content = re.sub(r'case StateConstants.AttackState:', 'case StateConstants.MeleeAttackState:\n            case StateConstants.RangedAttackState:', content)

# Change WindUpState transition to use the exported property NextStateAfterWindup
content = re.sub(
    r'case StateConstants\.WindUpState:\s+if \(p_reason == StateExitReason\.Finished\)\s+\{\s+nextStateName = StateConstants\.AttackStateName;\s+\}\s+break;',
    '''case StateConstants.WindUpState:
                if (p_reason == StateExitReason.Finished)
                {
                    if (p_sourceState is IslandSurvivor.Logic.StateMachine.States.WindUpState windUp)
                    {
                        nextStateName = windUp.NextStateAfterWindup;
                    }
                    else
                    {
                        nextStateName = StateConstants.MeleeAttackStateName;
                    }
                }
                break;''',
    content
)

with open(file_path, 'w') as f:
    f.write(content)
