# Combat System Architecture

## Overview
The combat system for IslandSurvivor uses a unified Area of Effect (AoE) attack mechanic. A single action (attacking with the spacebar) allows the player to damage both enemies and gatherable resources simultaneously, provided they are within the player's weapon hitbox.

## Key Interfaces
*   **`IDamageable`** (`Src/Core/Interfaces/Stats/IDamageable.cs`): Defines the contract for any entity that can take damage.
    *   `void TakeDamage(int p_amount, object p_attacker);`
    *   This interface is implemented by aggressive NPCs (e.g., `Soldier.cs`), passive NPCs (e.g., `Sheep.cs`), and Resources (e.g., `Rock.cs`, `Gold.cs`, `ConiferTree.cs`, `AutomnTree.cs`).

## Damage Resolution Flow
1.  **Attack Trigger**: The player presses the attack input. `Player.cs` enters the `Attacking` state.
2.  **Target Acquisition**: The script immediately retrieves all overlapping areas (`GetOverlappingAreas()`) and bodies (`GetOverlappingBodies()`) currently intersecting with the player's `m_weaponArea` (`Area2D`).
3.  **Filtration and Deduplication**: The script checks if each overlapping object (or its parent node) implements `IDamageable`. It uses a `HashSet<IDamageable>` to guarantee that an entity with multiple overlapping colliders only receives damage once per attack execution.
4.  **Damage Calculation**: The damage amount is retrieved from the player's `StatManager` (`StatType.Attack`).
5.  **Damage Application**: `TakeDamage(amount, playerInstance)` is invoked on each valid target in the `HashSet`.

## Subsystem Integrations
*   **StatManager**: Used by both the attacker (to determine outgoing damage) and the defender (to deduct health from `StatType.Health`).
*   **InventorySystem**: When a resource's health reaches 0, it instantiates a `ResourceItem` and broadcasts its destruction via `SignalManager.Instance.EmitMaterialDestroyed()`. The global `InventoryNode` listens to this signal and increments the player's inventory.
*   **ScoreManager**: When an enemy (like `Soldier`) dies, it notifies the core `ScoreTracker` via `ServiceRegistry.Instance.ScoreTracker.AddScore(int)` to increment the player's score.