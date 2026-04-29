# Line of Sight Implementation

This document outlines the Line of Sight (LoS) feature implemented for aggressive NPCs in IslandSurvivor.

## Overview
The Line of Sight feature allows an aggressive NPC (like the `Soldier`) to detect the player within a certain radius and chase them, but only if there is a clear, unobstructed path. If the player breaks the line of sight (e.g., hiding behind a wall) for a configurable duration, the NPC will abandon the chase and return to an idle/wandering state. This encourages stealthy tactics.

## Architecture & Components

The implementation follows a strict N-Tier architecture, splitting Godot-specific interaction (Physics/Nodes) from state management (Core Logic).

### Godot Client (`Soldier.cs`)
The Godot script is responsible for sensing the environment. It utilizes two main Godot nodes:

1. **`DetectionArea` (Area2D):** Detects when the player enters or exits a predefined detection radius.
2. **`LineOfSightRay` (RayCast2D):** A raycast that continuously points towards the target player when they are in the detection radius. It checks for collisions along the path. If it hits an obstacle (like a wall), the line of sight is considered broken.

**Usage:**
In the `_PhysicsProcess`, the Godot script calls `CheckLineOfSight()` to evaluate if the RayCast can clearly see the player. It then passes the boolean result to the `AgressorController` in the logic layer.

### Core Logic (`AgressorController.cs`)
The core logic manages the state transitions without any knowledge of Godot Nodes.

- **State Updates:** The `Update` method receives `p_hasLineOfSight`.
- **Timer Management:** When line of sight is broken, a `m_disengageTimer` starts (currently set to 3.0 seconds).
- **State Transition:** If the timer reaches 0 before the player is seen again, the state transitions from `NpcStates.CHASE` back to `NpcStates.IDLE`. If the player is spotted again before the timer expires, the timer resets and the chase continues.

## Setup Instructions for Godot Editor
For the LoS to work correctly, the Godot scene for the aggressive NPC must be configured as follows:

1. Add an `Area2D` child node named `DetectionArea`.
   - Add a `CollisionShape2D` child to the `Area2D` and configure its shape to match the desired detection radius.
2. Add a `RayCast2D` child node named `LineOfSightRay`.
   - Ensure it is enabled.
   - Configure the collision mask so that it only collides with obstacles (e.g., walls) and optionally the player, but ignores other enemies or ground tiles.
