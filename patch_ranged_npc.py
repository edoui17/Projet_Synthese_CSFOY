file_path = 'Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/RangedAggressiveNpcBase.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

# RangedAggressiveNpcBase transitions to RangedAttackState via WindUp, so let's check its WindUp
content = re.sub(
    r'if \(m_attackController != null && m_attackController\.CanAttack\)\s*\{\s*return IslandSurvivor\.Logic\.StateMachine\.StateConstants\.WindUpStateName;\s*\}',
    '''var shooter = GetNodeOrNull<IslandSurvivor.Nodes.Combat.Shooter>("Shooter");
        if (shooter != null && shooter.CanShoot)
        {
            var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
            if (windUp != null)
            {
                windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.RangedAttackStateName;
            }
            return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
        }
        else if (m_attackController != null && m_attackController.CanAttack)
        {
            var windUp = m_stateMachine?.GetState(IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName) as IslandSurvivor.Logic.StateMachine.States.WindUpState;
            if (windUp != null)
            {
                windUp.NextStateAfterWindup = IslandSurvivor.Logic.StateMachine.StateConstants.RangedAttackStateName;
            }
            return IslandSurvivor.Logic.StateMachine.StateConstants.WindUpStateName;
        }''',
    content
)

# Remove ShootProjectile
content = re.sub(r'\[Export\] public PackedScene ProjectileScene \{ get; set; \} = null!;\s+', '', content)
content = re.sub(r'm_attackController\.AttackActionTriggered \+= ShootProjectile;', '', content)
content = re.sub(r'protected virtual void ShootProjectile\(\)\s*\{[\s\S]*?\}\s*\}', '', content)

with open(file_path, 'w') as f:
    f.write(content)
