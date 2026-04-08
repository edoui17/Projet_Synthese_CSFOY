# Wiki - Player Systems: Movement & Interaction

This page documents the player character's locomotion and interaction architecture for the **IslandSurvivor** project.

---

## 1. Player Movement Architecture

The movement system is designed using a simple and direct approach for responsive controls.

### Character States (Project IslandSurvivor)
*   **PlayerState**: A project-specific enum (`Src/IslandSurvivor/Enums/PlayerState.cs`) that defines character activity: `Idle`, `Moving`, `Interacting`, and `Attacking`. This allows the engine to manage animation and movement logic based on the current context.

### Configuration
*   **PlayerStats** (`PlayerStats.tres`): Manages character attributes like `Speed`, linked to the `StatManager` node.

### Locomotion Implementation (Godot Client)
The `Player.cs` script inherits from `CharacterBody2D` and uses `MoveAndSlide()`.
*   **Constant Speed**: Movement is handled by directly setting the velocity based on the input vector and the character's speed stat. There is no acceleration or friction, ensuring immediate response to player input.
*   **State Machine**: The state is updated based on current velocity and input, triggering animations automatically via the `AnimationPlayer`.

---

## 2. Interaction System

The interaction system handles detection and prioritized decision-making within the game client.

### Interfaces and Services (Project IslandSurvivor)
*   **IInteractable** (`Src/IslandSurvivor/Interfaces/IInteractable.cs`): An interface required for any object that wants to be triggered or entered. It defines distance calculation, the interaction trigger, and the prompt text.
*   **InteractionService** (`Src/IslandSurvivor/Managers/InteractionService.cs`): This service receives the list of nearby interactable objects and the player's position. It performs the distance calculations and returns the single prioritized target. This ensures that even if several objects overlap, only one action is triggered at a time.

### Proximity Detection
*   **PlayerInteraction (Area2D)**: The player character has a dedicated detection zone. Any `InteractableNode` entering this zone is added to a local list of candidates.
*   **Visual Indicator**: A floating label on the player displays the prompt of the "best" (closest) interactable object in range.

---

## 3. Input Mapping

The following inputs are configured in `project.godot`:
*   **WASD / Arrow Keys**: Movement axes (Up, Down, Left, Right).
*   **E Key**: Interaction trigger.
*   **Space**: Attack action.
