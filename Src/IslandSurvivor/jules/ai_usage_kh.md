# IslandSurvivor - AI Usage & Collaboration Journal (KH)

This document tracks the interaction between Kevin Houle and the AI (Forge) to document the design process and AI's role in the project.

---
## Usage History

### 2026-03-29 - Setup of the "Forge" Assistant
- **Request:** Create a comprehensive system prompt for an AI assistant (Forge) to help develop the "IslandSurvivor" Roguelike project.
- **AI Contribution:** Defined the architectural boundaries (N-Tier), coding conventions (prefixing fields with m_ and parameters with p_), and established a rigorous journaling process.
- **Decision Reasoning:** Chose a Persona-based approach (Forge) to ensure consistency in code style and architectural integrity, specifically separating shared logic (Core) from presentation (Godot/Blazor).

### 2026-03-29 - [User Story 5.3] Gestionnaire de signaux global (SignalManager)
- **Request:** Implement a Global SignalManager (Autoload) to ensure fluid communication between game systems without memory leaks.
- **AI Contribution:** Designed and implemented a `WeakEvent` pattern in C# (Core) using `WeakReference` to allow the Garbage Collector to automatically clean up destroyed Godot Nodes. Created `ISignalManager` and its implementations. Wrote xUnit tests to validate the weak references. Avoided `var` keywords and only put a commented example in the interface. Subsequently refactored classes to be strictly separated by file (e.g. `WeakEvent` and `WeakEventNonGeneric`), and applied strict member ordering (fields, then constructors, then properties).
- **Decision Reasoning:** Chose pure C# events with `WeakReference` over native Godot signals to decouple the `Core` logic from the Godot Engine and allow thorough unit testing of the communication architecture without spinning up the Godot environment. The strict code layout and isolation into single files was enforced to maintain maximum clarity and homogeneity.

### 2026-03-30 - [User Story 5.1] Implémenter le gestionnaire de statistiques
- **Request:** Implement the Entity Stats Manager tracking Base and Effective stats (Health, Attack, Luck, Speed), preventing negative numbers or over-healing, and ensuring notifications on value updates. Must follow N-Tier architecture logic.
- **AI Contribution:** Built an Enum-based stat tracker purely in pure C# `/Src/Core`, implemented `StatTracker` and `Stat` classes tracking Base vs Current vs Max values using the `WeakEvent` cross-system communication. In Godot, exposed read-only `[GlobalClass]` config via `EntityStats` `Resource`, loaded natively by `StatManager` node via the Bridge pattern, re-emitting `WeakEvent` signals into visual Godot Editor native `[Signal]`. Covered entirely by new xUnit tests.
- **Decision Reasoning:** Hybrid Bridge approach chosen to keep the engine `Core` completely ignorant of Godot concepts (making tests easier) while fully restoring Godot Editor functionality. Read-only `EntityStats` prevent accidental cross-entity modification and allow `StatManager` logic safely.
