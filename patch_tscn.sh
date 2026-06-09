#!/bin/bash

# Remove DetectionArea and its children from AggressiveNpcBase.tscn
sed -i -e '/\[node name="DetectionArea"/,/^$/d' Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.tscn
# Remove SubResource("CircleShape2D_det") if it exists
sed -i -e '/\[sub_resource type="CircleShape2D" id="CircleShape2D_det"\]/,/^$/d' Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.tscn

# Remove DetectionArea references from other .tscn files
files=(
    "Src/IslandSurvivor/Scenes/NPC/Aggressive/Boss/Reaper/Reaper.tscn"
    "Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Lancer/Lancer.tscn"
    "Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Soldier/Soldier.tscn"
    "Src/IslandSurvivor/Scenes/NPC/Aggressive/Normal/Archer/Archer.tscn"
)

for file in "${files[@]}"; do
    if [ -f "$file" ]; then
        sed -i -e '/\[node name="DetectionArea"/,/^$/d' "$file"
        # The CollisionShape2D inside DetectionArea might not be caught if it's separated by empty lines,
        # but usually it's adjacent or just below. Let's do a more robust removal for inherited scenes:
        sed -i -e '/\[node name="CollisionShape2D" parent="DetectionArea"/,/^$/d' "$file"
    fi
done
