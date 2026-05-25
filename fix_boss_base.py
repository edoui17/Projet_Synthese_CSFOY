file_path = 'Src/IslandSurvivor/Scenes/NPC/Aggressive/Boss/BossBase.cs'
with open(file_path, 'r') as f:
    content = f.read()

# Fix syntax error caused by regex
content = content.replace("    }\n\n    \n    }", "    }")

with open(file_path, 'w') as f:
    f.write(content)
