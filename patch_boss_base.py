import re

file_path = 'Src/IslandSurvivor/Scenes/NPC/Aggressive/Boss/BossBase.cs'
with open(file_path, 'r') as f:
    content = f.read()

# Remove ProjectileScene and ShootProjectile logic as it's now handled by Shooter node
content = re.sub(r'\[Export\] public PackedScene ProjectileScene \{ get; set; \} = null!;\s+', '', content)
content = re.sub(r'protected virtual void ShootProjectile\(\)\s*\{[^}]*\}', '', content, flags=re.DOTALL)
# One more time for any inner blocks of ShootProjectile
content = re.sub(r'protected virtual void ShootProjectile\(\)\s*\{[\s\S]*?\}\s*\}', '', content)
content = re.sub(r'ShootProjectile\(\);', '', content)

with open(file_path, 'w') as f:
    f.write(content)
