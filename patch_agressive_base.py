import re
file_path = 'Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.cs'
with open(file_path, 'r') as f:
    content = f.read()

content = re.sub(
    r'if \(m_attackController != null && m_attackController\.CanAttack\)\s*\{\s*return IslandSurvivor\.Logic\.StateMachine\.StateConstants\.WindUpStateName;\s*\}',
    '''if (m_attackController != null && m_attackController.CanAttack)
        {
            var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
            if (windUp != null)
            {
                windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.MeleeAttackStateName;
            }
            return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
        }''',
    content
)

with open(file_path, 'w') as f:
    f.write(content)
