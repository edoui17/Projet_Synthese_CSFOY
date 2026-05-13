import re

with open('./Src/IslandSurvivor/Scenes/NPC/Agressive/Archer.cs', 'r') as f:
    content = f.read()

# Add missing namespace Godot.Collections if needed or NpcStates
old_handle = """if (m_agressorController.CurrentState == NpcStates.DEAD) return;"""
new_handle = """if (m_agressorController.CurrentState == IslandSurvivor.Logic.Entities.NpcStates.DEAD) return;"""

if "using IslandSurvivor.Logic.Entities;" not in content:
    content = content.replace("using Godot;", "using Godot;\nusing IslandSurvivor.Logic.Entities;")

with open('./Src/IslandSurvivor/Scenes/NPC/Agressive/Archer.cs', 'w') as f:
    f.write(content)
