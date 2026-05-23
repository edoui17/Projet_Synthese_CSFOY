# User Story 14 - Audio & Feedback Documentation

This document explains the steps required to configure the newly implemented Audio & Tween feedback features directly within the Godot Editor.

## 1. Setting up Audio Files (Task 14.1.1)

To ensure the new sounds play correctly in the game, you need to add your `.wav` files into the `res://Assets/Sounds/` directory matching the exact structure below. If you prefer different file names, you must update the C# paths in `ResourceDrop.cs`, `EnemyBase.cs`, `Sheep.cs`, etc.

### Required Audio Files:
*   **Combat:**
    *   `res://Assets/Sounds/Combat/wood_impact.wav`
    *   `res://Assets/Sounds/Combat/wood_destroy.wav`
    *   `res://Assets/Sounds/Combat/rock_impact.wav`
    *   `res://Assets/Sounds/Combat/rock_destroy.wav`
    *   `res://Assets/Sounds/Combat/enemy_hurt.wav`
    *   `res://Assets/Sounds/Combat/enemy_death.wav`
    *   `res://Assets/Sounds/Combat/enemy_attack.wav`
    *   `res://Assets/Sounds/Combat/animal_hurt.wav`
    *   `res://Assets/Sounds/Combat/animal_death.wav`
    *   `res://Assets/Sounds/Combat/player_hurt.wav`
    *   `res://Assets/Sounds/Combat/player_death.wav`
    *   `res://Assets/Sounds/Combat/weapon_swing.wav`

*   **UI / Systems:**
    *   `res://Assets/Sounds/UI/stat_upgrade.wav` (Already exists/configured)

## 2. ResourceDrop Scene Configuration (Task 14.1.3 & 14.1.4)

A new visual effect has been added using Tweens to simulate materials flying to the player when a resource is destroyed. We need to create the corresponding `.tscn` file.

**Steps to create `ResourceDrop.tscn`:**
1. In the Godot FileSystem, navigate to `Src/IslandSurvivor/Scenes/Ressources/`.
2. Right-click -> **Create New Scene**. Select **Node2D** as the root node.
3. Rename the root node to `ResourceDrop`.
4. Attach the script `Src/IslandSurvivor/Scenes/Ressources/ResourceDrop.cs` to the root node.
5. Save the scene exactly as `res://Scenes/Ressources/ResourceDrop.tscn` (or matching the exact path expected by the `GD.Load<PackedScene>` in the `ConiferTree`, `AutomnTree`, `Rock`, `Gold`, `Sheep` scripts, which is currently programmed to look for `res://Scenes/Ressources/ResourceDrop.tscn`. Adjust folder nesting if necessary).

*Note: The script dynamically handles generating the sprite and setting its texture based on the `ResourceItem` icon path, so you do not need to manually add a `Sprite2D` to this scene.*

## 3. Portal Setup (Scenario 4)

The portal interaction logic currently uses a `Tween` to fade in audio and modify shader modulate parameters.

*   Ensure the Portal parent node has an `AudioStreamPlayer2D` node named exactly `AmbientAudio`. Provide it with a looping portal hum `.wav`.
*   Ensure the Portal has a `Sprite2D` node with a `ShaderMaterial` attached. The code currently tweens the `modulate` property to simulate activation (`TweenProperty(portalSprite, "modulate", new Color(2f, 2f, 2f, 1f), 1.0f)`).