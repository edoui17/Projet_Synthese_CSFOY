import re

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Boss/BossBase.cs', 'r') as f:
    content = f.read()

replacement = """    protected Area2D? m_hitboxAreaRight;
    protected Area2D? m_hitboxAreaLeft;

    protected override Godot.StringName GetCombatDecisionState(float distanceSquared, float attackRangeSquared)
    {
        // Future boss phases and AoE cooldown logic will be injected here.
        return base.GetCombatDecisionState(distanceSquared, attackRangeSquared);
    }"""

content = re.sub(r'    protected Area2D\? m_hitboxAreaRight;\n    protected Area2D\? m_hitboxAreaLeft;', replacement, content)

with open('./Src/IslandSurvivor/Scenes/NPC/Aggressive/Boss/BossBase.cs', 'w') as f:
    f.write(content)
