import sys

def patch_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    search = """        if (m_wasKilledByPlayer)
        {
            int goldAmount = 2;
            ResourceItem goldResource = new ResourceItem("gold_coin", "Piece d'Or", "Gold Coin", "res://Assets/TinySwords/TinySwords(Update010)/Resources/Gold_Coin.png");"""

    replace = """        if (m_wasKilledByPlayer)
        {
            int goldAmount = 2;

            float luck = IslandSurvivor.Globals.ServiceRegistry.Instance.StatTracker.GetCurrentValue(Core.Managers.Stats.StatType.Luck);
            float bonusChance = luck * 0.05f;
            int bonusQuantity = (int)bonusChance;
            float fractionalChance = bonusChance - bonusQuantity;

            Random random = new();
            if (random.NextDouble() < fractionalChance)
            {
                bonusQuantity++;
            }

            goldAmount += bonusQuantity;

            ResourceItem goldResource = new ResourceItem("gold_coin", "Piece d'Or", "Gold Coin", "res://Assets/TinySwords/TinySwords(Update010)/Resources/Gold_Coin.png");"""

    if search in content:
        content = content.replace(search, replace)
        with open(filepath, 'w') as f:
            f.write(content)
        print(f"Success: {filepath}")
    else:
        print(f"Search string not found in {filepath}")

patch_file('./Src/IslandSurvivor/Scenes/NPC/Agressive/Soldier.cs')
