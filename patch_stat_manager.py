import re

with open("Src/IslandSurvivor/Nodes/StatsManager/StatManager.cs", "r") as f:
    content = f.read()

# Add using IslandSurvivor.Enums;
content = re.sub(r'using IslandSurvivor.Resources;', 'using IslandSurvivor.Resources;\nusing IslandSurvivor.Enums;', content)

# Add m_entityType export
export_group_idx = content.find('[ExportGroup("Base Stats")]')
export_entity_type = """[Export]
    private EntityType m_entityType = EntityType.NPC;

    """
content = content[:export_group_idx] + export_entity_type + content[export_group_idx:]

# Modify _Ready()
old_init_stats = """Dictionary<StatType, float> initialStats = new Dictionary<StatType, float>
        {
            { StatType.Health, MaxHealth },
            { StatType.Attack, 0f }, // Stat points are 0 by default, modified by permanent upgrades
            { StatType.Speed, 0f }, // Stat points are 0 by default
            { StatType.Luck, Luck }
        };"""

new_init_stats = """Dictionary<StatType, float> initialStats = new Dictionary<StatType, float>
        {
            { StatType.Health, MaxHealth }
        };

        if (m_entityType == EntityType.NPC || m_entityType == EntityType.Player)
        {
            initialStats.Add(StatType.Attack, 0f);
            initialStats.Add(StatType.Speed, 0f);
        }

        if (m_entityType == EntityType.Player)
        {
            initialStats.Add(StatType.Luck, Luck);
        }"""

content = content.replace(old_init_stats, new_init_stats)

with open("Src/IslandSurvivor/Nodes/StatsManager/StatManager.cs", "w") as f:
    f.write(content)
