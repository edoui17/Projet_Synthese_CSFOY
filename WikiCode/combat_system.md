# Combat System Architecture

## Overview
The combat system for IslandSurvivor uses a unified Area of Effect (AoE) attack mechanic. A single action (attacking with the spacebar) allows the player to damage both enemies and gatherable resources simultaneously, provided they are within the player's weapon hitbox.

## Key Interfaces
*   **`IDamageable`** (`Src/Core/Interfaces/Stats/IDamageable.cs`): Defines the contract for any entity that can take damage.
    *   `void TakeDamage(int p_amount, Core.Domain.DamageContext p_context);`
    *   This interface is implemented by aggressive NPCs (e.g., `Soldier.cs`), passive NPCs (e.g., `Sheep.cs`), and Resources (e.g., `Rock.cs`, `Gold.cs`, `ConiferTree.cs`, `AutomnTree.cs`). The `DamageContext` allows the system to pass the attacker reference along with calculated damage and specific stat snapshots (like Luck) without forcing the victim to read another entity's `StatManager`.

## Damage Resolution Flow (Player Attacking Enemy/Resource)
1.  **Attack Trigger**: The player presses the attack input. `Player.cs` enters the `Attacking` state.
2.  **Target Acquisition**: The script relies on event-driven signals (`AreaEntered`, `BodyEntered`) from the player's `m_weaponAreaRight` / `m_weaponAreaLeft` (`Area2D`).
3.  **Filtration and Deduplication**: The script checks if each overlapping object (or its parent node) implements `IDamageable`. It uses a `HashSet<IDamageable>` to guarantee that an entity with multiple overlapping colliders only receives damage once per attack execution.
4.  **Damage Calculation**: The damage amount is calculated based on the base damage and retrieved from the player's `StatManager` (`StatType.Attack`).
5.  **Damage Application**: `TakeDamage(amount, new DamageContext(...))` is invoked on each valid target in the `HashSet`.

## Damage Resolution Flow (Enemy Attacking Player)
1.  **Hitbox Trigger**: The aggressive enemy (e.g., `Soldier` or `Archer`) has an `Area2D` named `HitboxArea` used to detect the player.
2.  **Target Acquisition**: When a body enters the `HitboxArea`, the `BodyEntered` signal fires.
3.  **Verification & Damage Application**: The script verifies if the colliding body is in the "Player" group and implements `IDamageable`. If true, the enemy immediately calls `TakeDamage(amount, new DamageContext(...))` on the player. The base damage is configured to scale properly starting from Level 1 for these entities. The player's `StatManager` then deducts the corresponding health.

## Subsystem Integrations
*   **StatManager**: Since the major stats refactoring, every entity has an isolated `StatManager` utilizing a local `EventBus`. The attacker determines its outgoing damage (Base Damage + Attack multiplier). The defender deducts health and uses the `LocalStatChanged` signal to trigger its death sequence if health reaches 0.
*   **InventorySystem**: When a resource (or an enemy with drops) reaches 0 health, it receives the `DamageContext` containing the attacker's `Luck` bonus, instantiates a `ResourceItem`, and broadcasts its destruction via `SignalManager.Instance.EmitMaterialDestroyed()`. The global `InventoryNode` listens to this signal and increments the player's inventory.
*   **ScoreManager**: When an aggressive enemy (like `Soldier`) dies, it notifies the core `ScoreTracker` via `ServiceRegistry.Instance.ScoreTracker.AddScore(int)` to increment the player's score.
## Enemy Combat System (Melee)

The enemy combat system in IslandSurvivor follows the N-Tier architecture, separating business logic from the Godot client representation.

### Core Logic (Src/Core)
- **Interfaces:** `IAttackable`, `IDamageable` dictate the fundamental interaction for dealing and receiving damage.
- **Controllers:** `IAgressorController` handles the state machine for aggressive entities. It tracks states (`IDLE`, `CHASE`, `ATTACK`, `DEAD`) and manages attack cooldowns and durations purely in C# logic, oblivious to Godot frames.

### Godot Client (Src/IslandSurvivor)
- **Hit Detection:** Melee attacks use dedicated `Area2D` nodes (`HitboxArea` on enemies, `WeaponAttack` on players).
- **Target Tracking:** Entities utilize `BodyEntered` and `BodyExited` signals to maintain a `HashSet<IDamageable>` of currently overlapping targets. This approach is more reliable than polling `GetOverlappingBodies()` mid-animation.
- **Visuals:** The `_PhysicsProcess` queries the Controller's current state. If the state is `ATTACK`, movement is halted, and the `AnimatedSprite2D` transitions to the "Attack" animation.
- **Stats Integration:** Damage calculations and health modifications are processed through the attached `StatManager`, ensuring all entity stats are centralized and driven by `EntityStats` resources.
