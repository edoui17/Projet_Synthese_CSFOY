# Progression Loop Architecture

## Overview
The progression system is built upon a hybrid architecture that leverages pure C# Core rules combined with Godot-specific node orchestrators. The goal is to provide a clean loop where the player gains XP from specific activities (harvesting, killing enemies, exploring), levels up, and eventually unlocks the final Boss Island.

## N-Tier Responsibilities

### 1. The "Accountant" (`Core.Managers.Stats.StatTracker`)
The Core layer is strictly responsible for math and raw data storage.
- Tracks current XP (`Experience`) and Current Level (`Level`) via `StatType`.
- Implements the **Triangular Progression Formula**: `Required XP = 50 + (CurrentLevel * 50)`.
- Exposes `AddExperience(float)` which recalculates the state and automatically loops through potential multiple level-ups.
- Emits agnostic events (`ExperienceGainedEvent`, `LevelChangedEvent`) via the `IEventBus`.
- Core has zero knowledge of Godot nodes, enemies, or scenes.

### 2. The "Orchestrator" (`IslandSurvivor.Managers.ProgressionManager`)
Located in the game client (Godot), this global Autoload acts as the bridge between gameplay and the Core.
- Subscribes to events triggered by gameplay: `ResourceHarvestedEvent`, `EnemyKilledEvent`, and `NavigationRequestedEvent`.
- Determines the *value* of these actions (e.g., harvesting gives 10 XP, killing an enemy gives 30 XP, traveling to a new island gives 50 XP).
- Injects these values back into the Core by calling `StatTracker.AddExperience()`.
- Exposes `IsBossReady()` by comparing the Player's level with the requirements set in the `ProgressionRequirement` resource.

### 3. The "Configuration" (`IslandSurvivor.Resources.ProgressionRequirement`)
A pure Godot Resource (`.tres`) that allows designers to edit requirements without touching the C# scripts. Currently tracks the `BossLevelRequirement` (Default: Level 10).

## UI Flow
1. **Player acts** (e.g., destroys a `Soldier` node).
2. **Event Emission**: The node publishes an `EnemyKilledEvent` via the `EventBus`.
3. **Orchestrator computes**: `ProgressionManager` intercepts the event, decides the reward (30 XP), and calls `StatTracker.AddExperience(30)`.
4. **Core updates**: `StatTracker` increments XP, determines a level-up has occurred, and publishes `LevelChangedEvent` and `ExperienceGainedEvent`.
5. **UI reacts**: `Player.cs` (via subscribed listeners) updates the `LevelLabel` and triggers a temporary `XpGainLabel` pop-up to provide visual feedback to the user.

## Boss Island Unlocking
- Handled locally inside `NavigationMenu.cs`.
- The menu queries `ProgressionManager.Instance.IsBossReady()`.
- If `false`, the button is generated but visibly locked/disabled, indicating the requirement (Level 10).
- If `true`, the button unlocks and provides access to the final boss level.
