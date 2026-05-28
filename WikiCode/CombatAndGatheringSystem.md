# Combat and Gathering System Architecture

## Overview
The IslandSurvivor project utilizes a unified system for both combat and resource gathering. The core philosophy is that an Area of Effect (AoE) attack launched by the player interacts indiscriminately with both enemies and resources, processing hit logic identically using shared .NET 8 Core logic.

## Key Interfaces and Systems
- **`IDamageable`**: Defined in `Src/Core/Interfaces/Stats/IDamageable.cs`. Represents any entity that can take damage (`void TakeDamage(int p_amount, object p_attacker)`). Both enemies (e.g., `Soldier`, `Sheep`) and resources (e.g., `Gold`, `AutomnTree`) implement this interface.
- **`StatManager`**: Handles health, attack, speed, and luck values. When the player deals damage, it pulls its Attack value from its `StatManager` and reduces the target's Health value in their `StatManager`.
- **Hit Detection**: The Player's weapon (`Area2D`) uses `GetOverlappingAreas()` or body/area signals. Targets are stored in a `HashSet<IDamageable>` for the duration of the attack animation to prevent multi-hit glitches from Godot returning both a body and an area for a single entity.
- **Visual Feedback**: All damageable entities utilize the `PlayHitFlash()` generic extension method defined in `NodeExtensions.cs` to pulse red briefly when taking damage.

## Workflow

### 1. Attacking
When the player attacks (`Espace` / Spacebar), the attack animation plays and the weapon `Area2D` detects targets on **Collision Layer 5 (Value 16)**. The player is on Layer 4 (Value 8).
The player's script applies damage to all discovered `IDamageable` entities in the area simultaneously.

### 2. Destruction (Resources)
When a resource's HP drops to 0 or below, its local `StatManager` fires a `LocalStatChanged` signal which triggers `DestroyResource(object p_attacker)`.
- It extracts the `StatManager` from the `p_attacker` to calculate the `Luck` bonus.
- It instantiates a `ResourceItem`.
- It invokes `SignalManager.Instance.EmitMaterialDestroyed(...)` with the calculated bonus amount.
- `InventoryNode.cs` intercepts the signal and adds the dropped materials to the player's inventory using `IInventoryManager`.
- The resource calls `QueueFree()`.

### 3. Death (Enemies)
When an enemy's HP drops to 0 or below, its local `StatManager` fires a `LocalStatChanged` signal which triggers `HandleDeath(object p_attacker)`.
- The death animation/logic plays via its specific controller (e.g., `StateMachine`).
- Entities like Sheep and Soldiers extract the `p_attacker`'s `Luck` stat to calculate their resource drop quantity and notify the player via `EmitMaterialDestroyed`.
- Aggressive enemies like Soldiers notify the `ScoreManager` (`ServiceRegistry.Instance.ScoreTracker.AddScore(...)`) to grant the player points.
- The enemy calls `QueueFree()`.
