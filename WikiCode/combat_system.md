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
*   **StatManager**: Since the major stats refactoring, every entity has an isolated `StatManager` utilizing a local `EventBus`. The attacker determines its outgoing damage (Base Damage + Attack multiplier). The defender deducts health and uses the `LocalStatChanged` signal to trigger its death sequence if health reaches 0.
*   **InventorySystem**: When a resource (or an enemy with drops) reaches 0 health, it receives the `p_attacker` object. It extracts the attacker's local `StatManager` to calculate the `Luck` bonus, instantiates a `ResourceItem`, and broadcasts its destruction via `SignalManager.Instance.EmitMaterialDestroyed()`. The global `InventoryNode` listens to this signal and increments the player's inventory.
*   **ScoreManager**: When an aggressive enemy (like `Soldier`) dies, it notifies the core `ScoreTracker` via `ServiceRegistry.Instance.ScoreTracker.AddScore(int)` to increment the player's score.