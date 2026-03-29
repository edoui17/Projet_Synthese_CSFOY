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

### Strict C# Coding Conventions - Project Enforcements
- **Context:** Formatting classes and structures across the C# projects (`Core`, `IslandSurvivor`).
- **Finding:** In order to maintain absolute consistency, implicit typing and mixed structural layouts are strictly prohibited.
- **Resolution:** All classes must adhere to the following rules moving forward:
  1. **Typing:** The `var` keyword is strictly forbidden. All variables must be explicitly typed (e.g., `List<string> list = new List<string>()`).
  2. **Class Isolation:** Every distinct class, even non-generic counterparts (e.g., `WeakEvent` and `WeakEvent<T>`), must reside in its own separate file.
  3. **Member Ordering:** Inside any given class, the order must strictly be:
     - **1. Member Variables** (fields starting with `m_`) at the absolute top.
     - **2. Constructors** immediately following the member variables.
     - **3. Properties** (e.g., `public int Value { get; set; }`) immediately following the constructors.
     - **4. Methods** at the end.