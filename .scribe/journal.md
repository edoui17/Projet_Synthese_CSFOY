## 2026-05-20 - [Missing Core Architectural Guide]
**Observation:** The 'Accountant vs. Orchestrator' architectural boundary is a core pillar of the project (mentioned in Convention.md and forges.md), but lacks a dedicated master reference in the WikiCode folder to explain the *why* and *how* to new developers.
**Action:** Created `WikiCode/Accountant_vs_Orchestrator.md` to serve as the master guide, standardizing how we document the separation between pure C# Core logic and Godot Client nodes, and updated outdated terminology (e.g., BaseDamage to BaseAttackValue) in existing files.

## 2026-05-27 - [Outdated Godot Client Node References and Architecture Discrepancy]
**Observation:** The documentation for enemy controllers and state management referred to `AgressorController` and `AnimatedSprite2D`, which have been replaced by the new Godot Node-based `StateMachine` architecture with `Sprite2D` and `AnimationPlayer`. In addition, some scripts still contained outdated `GetNode<StatManager>` logic instead of relying on explicit Exports.
**Action:** Replaced mentions of `AgressorController` with `StateMachine`, `IAgressorController` with `IStateMachine`, updated `AnimatedSprite2D` references to use `Sprite2D` and `AnimationPlayer`, and corrected Godot script API syntax (`GetNode<StatManager>("StatManager")` to `Stats`) across the wiki pages to match current code logic.
