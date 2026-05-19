# Combat System Architecture

The combat system is unified around the `AttackController` Godot node, which acts as the spatial and temporal brain of the attack mechanics.

## Responsibilities

*   **AttackController (Godot Node):** Manages attack cooldowns and active durations (modified by the `Speed` stat). It activates and deactivates registered `Area2D` hitboxes and uses an internal list of hit targets to prevent multi-hit issues per attack. It emits signals (`AttackStarted`, `TargetHit`, `AttackFinished`) to decouple visual and audio effects from the logic.
*   **CombatMath (C# Utility):** Centralizes the mathematical formulas for combat. Currently resolves final damage using the `Attack` stat and calculates duration/cooldown reductions using the `Speed` stat.
*   **Factions:** Uses `EntityFaction` to determine valid targets. `Player` faction damages `Enemy` faction and resources (`IAttackable`). `Enemy` faction damages only the `Player`.

## Core Separation

The Core logic (`AgressorController`) no longer manages attack timers. Time-based physical interactions are exclusively managed in the Godot client layer by `AttackController`.

## Dash Interruption

Projectiles (like arrows) interact with the `MovementController`. If a projectile hits a target that is currently dashing, the dash is interrupted (`CancelDash()`), and a brief stun is applied (`ApplyStun(float duration)`), zeroing out velocity.
