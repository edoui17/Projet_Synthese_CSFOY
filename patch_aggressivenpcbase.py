import re

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.cs', 'r') as f:
    content = f.read()

replacement = """    [ExportGroup("Melee Configuration")]
    [Export] public float AttackRange { get; set; } = 60.0f;
    [Export] public float GuardChance { get; set; } = 0.3f;

    [ExportGroup("Ranged Configuration")]
    [Export] public float MinAttackRange { get; set; } = 80.0f;
    [Export] public float MaxAttackRange { get; set; } = 350.0f;

    protected Node2D? m_targetPlayer;"""

content = re.sub(r'    protected Node2D\? m_targetPlayer;', replacement, content)

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.cs', 'w') as f:
    f.write(content)
