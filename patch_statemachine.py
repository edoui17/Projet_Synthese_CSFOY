import re

with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'r') as f:
    content = f.read()

# Remove the combat properties
content = re.sub(r'    \[ExportGroup\("Melee Configuration"\)\]\n    \[Export\] public float AttackRange \{ get; set; \} = 60\.0f;\n    \[Export\] public float GuardChance \{ get; set; \} = 0\.3f;\n\n    \[ExportGroup\("Ranged Configuration"\)\]\n    \[Export\] public float MinAttackRange \{ get; set; \} = 80\.0f;\n    \[Export\] public float MaxAttackRange \{ get; set; \} = 350\.0f;\n\n', '', content)

with open('./Src/IslandSurvivor/Logic/StateMachine/StateMachine.cs', 'w') as f:
    f.write(content)
