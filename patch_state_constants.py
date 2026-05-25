import re

file_path = 'Src/IslandSurvivor/Logic/StateMachine/StateConstants.cs'
with open(file_path, 'r') as f:
    content = f.read()

# Replace AttackState with MeleeAttackState
content = re.sub(r'public const string AttackState = "AttackState";', 'public const string MeleeAttackState = "MeleeAttackState";', content)
content = re.sub(r'public static readonly StringName AttackStateName = new StringName\(AttackState\);', 'public static readonly StringName MeleeAttackStateName = new StringName(MeleeAttackState);', content)

# Add RangedAttackState
content = re.sub(r'public const string MeleeAttackState = "MeleeAttackState";', 'public const string MeleeAttackState = "MeleeAttackState";\n    public const string RangedAttackState = "RangedAttackState";', content)
content = re.sub(r'public static readonly StringName MeleeAttackStateName = new StringName\(MeleeAttackState\);', 'public static readonly StringName MeleeAttackStateName = new StringName(MeleeAttackState);\n    public static readonly StringName RangedAttackStateName = new StringName(RangedAttackState);', content)

# Replace any lingering AttackState instances inside logic
content = re.sub(r'AttackStateName', 'MeleeAttackStateName', content)
content = re.sub(r'AttackState', 'MeleeAttackState', content)

with open(file_path, 'w') as f:
    f.write(content)
