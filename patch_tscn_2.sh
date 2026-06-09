#!/bin/bash
sed -i -e '/\[node name="CollisionShape2D" type="CollisionShape2D" parent="DetectionArea"/,/^$/d' Src/IslandSurvivor/Scenes/NPC/Aggressive/AggressiveNpcBase.tscn
