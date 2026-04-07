# Wiki - Player Systems: Movement & Interaction

This page documents the player character's locomotion and interaction architecture for the **IslandSurvivor** project.

---

## 1. Player Movement Architecture

The movement system is designed using a hybrid approach between Godot's physics engine and a centralized state machine.

### Core Logic (Project Core)
*   **PlayerState**: A shared enum (`Src/Core/Domain/PlayerState.cs`) that defines character activity: `Idle`, `Moving`, `Interacting`, and `Attacking`. This allows for cross-layer synchronization (e.g., UI and Game Logic).

### Data-Driven Configuration (Resources)
Movement parameters are externalized as Godot Resources to allow for quick iteration:
*   **PlayerMovementData** (`PlayerMovementData.tres`): Controls the "feel" of the character (Acceleration, Friction).
*   **PlayerStats** (`PlayerStats.tres`): Manages character attributes like `Speed`, linked to the `StatManager` node.

### Locomotion Implementation (Godot Client)
The `Player.cs` script inherits from `CharacterBody2D` and uses `MoveAndSlide()`.
*   **Physics Processing**: Velocity is calculated each frame using `MoveToward` to provide smooth acceleration and deceleration (friction) based on input.
*   **State Machine**: The state is updated based on current velocity and input, triggering animations automatically via the `AnimationPlayer`.

---

## 2. Interaction System (N-Tier)

The interaction system strictly follows the N-Tier architecture, separating detection from decision-making.

### Interface First
*   **IInteractable** (`Src/Core/Interfaces/IInteractable.cs`): An interface required for any object that wants to be picked up, triggered, or entered. It defines distance calculation, the interaction trigger, and the prompt text.

### Proximity Detection (Godot Client)
*   **PlayerInteraction (Area2D)**: The player character has a dedicated detection zone. Any `InteractableNode` entering this zone is added to a local list of candidates.
*   **Visual Indicator**: A floating label on the player displays the prompt of the "best" (closest) interactable object in range.

### Decision Logic (Project Core)
*   **InteractionService** (`Src/Core/Managers/InteractionService.cs`): This service receives the list of nearby interactable objects and the player's position. It performs the distance calculations and returns the single prioritized target. This ensures that even if several objects overlap, only one action is triggered at a time.

### Creating New Interactables
To make an object interactable:
1.  Attach an **InteractableNode** (`Src/IslandSurvivor/Nodes/InteractableNode.cs`) to the scene.
2.  Assign an **InteractionPrompt** (e.g., "Press E to open chest").
3.  Connect to the **Interacted** signal to implement specific behavior (opening a chest, entering a house, etc.).

---

## 3. Input Mapping

The following inputs are configured in `project.godot`:
*   **WASD / Arrow Keys**: Movement axes (Up, Down, Left, Right).
*   **E Key**: Interaction trigger.
*   **Space**: Attack action.
