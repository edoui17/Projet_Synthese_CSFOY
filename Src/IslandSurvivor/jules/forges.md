# Forge's Journal - IslandSurvivor Technical Learnings

This file tracks critical architectural decisions, Godot 4.6.1 quirks, and N-Tier synchronization patterns to avoid repeating mistakes.

---

## Project Context
- **Engine:** Godot 4.6.1 (.NET 8 / C#)
- **Architecture:** N-Tier (Core, API, Client, Infrastructure, Web)
- **Genre:** Roguelike

## Critical Learnings
*(Forge will add entries here as the project progresses)*

### Architecture - Handling Signals in a N-Tier Godot Project
- **Context:** Implementing the Global `SignalManager`.
- **Finding:** A native Godot Signal handles dead references automatically (Disconnecting when `Node.QueueFree()` is called), but it couples logic to Godot's Engine space (`Node`, `Godot.Object`).
- **Resolution:** A pure C# implementation of the `WeakEvent` pattern using `WeakReference` in the `Core` project. This allows completely uncoupled testing (via xUnit), decoupling domain logic from Godot, and handles garbage-collected dead references gracefully without explicit Unsubscribe mechanisms required to avoid memory leaks. Godot `Autoload` simply delegates logic to the `Core` manager.