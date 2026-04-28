import sys

def patch_file(filepath, class_name):
    with open(filepath, 'r') as f:
        content = f.read()

    search = f"""    public void DestroyResource()
    {{
        Random random = new();
        int quantity = random.Next(1, 5);"""

    replace = f"""    public void DestroyResource()
    {{
        Random random = new();
        int quantity = random.Next(1, 5);

        float luck = IslandSurvivor.Globals.ServiceRegistry.Instance.StatTracker.GetCurrentValue(Core.Managers.Stats.StatType.Luck);
        float bonusChance = luck * 0.05f;
        int bonusQuantity = (int)bonusChance;
        float fractionalChance = bonusChance - bonusQuantity;

        if (random.NextDouble() < fractionalChance)
        {{
            bonusQuantity++;
        }}

        quantity += bonusQuantity;"""

    if search in content:
        content = content.replace(search, replace)
        with open(filepath, 'w') as f:
            f.write(content)
        print(f"Success: {filepath}")
    else:
        print(f"Search string not found in {filepath}")

patch_file('./Src/IslandSurvivor/Scenes/Ressources/Gold/Gold.cs', 'Gold')
patch_file('./Src/IslandSurvivor/Scenes/Ressources/Rock/Rock.cs', 'Rock')
patch_file('./Src/IslandSurvivor/Scenes/Ressources/Tree/AutomnTree.cs', 'AutomnTree')
patch_file('./Src/IslandSurvivor/Scenes/Ressources/Tree/ConiferTree.cs', 'ConiferTree')
