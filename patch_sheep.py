import sys

def patch_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    search = """        if (m_wasKilledByPlayer)
        {
            int meatAmount = 1;
            ResourceItem meatResource = new ResourceItem("meat_01", "Viande", "Meat", "res://Assets/TinySwords/TinySwords(Update010)/Deco/17.png");"""

    replace = """        if (m_wasKilledByPlayer)
        {
            int meatAmount = 1;

            float luck = IslandSurvivor.Globals.ServiceRegistry.Instance.StatTracker.GetCurrentValue(Core.Managers.Stats.StatType.Luck);
            float bonusChance = luck * 0.05f;
            int bonusQuantity = (int)bonusChance;
            float fractionalChance = bonusChance - bonusQuantity;

            Random random = new();
            if (random.NextDouble() < fractionalChance)
            {
                bonusQuantity++;
            }

            meatAmount += bonusQuantity;

            ResourceItem meatResource = new ResourceItem("meat_01", "Viande", "Meat", "res://Assets/TinySwords/TinySwords(Update010)/Deco/17.png");"""

    if search in content:
        content = content.replace(search, replace)
        with open(filepath, 'w') as f:
            f.write(content)
        print(f"Success: {filepath}")
    else:
        print(f"Search string not found in {filepath}")

patch_file('./Src/IslandSurvivor/Scenes/NPC/Passive/Sheep.cs')
