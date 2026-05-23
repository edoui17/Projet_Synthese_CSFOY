## 2026-05-20 - [Missing Core Architectural Guide]
**Observation:** The 'Accountant vs. Orchestrator' architectural boundary is a core pillar of the project (mentioned in Convention.md and forges.md), but lacks a dedicated master reference in the WikiCode folder to explain the *why* and *how* to new developers.
**Action:** Created `WikiCode/Accountant_vs_Orchestrator.md` to serve as the master guide, standardizing how we document the separation between pure C# Core logic and Godot Client nodes, and updated outdated terminology (e.g., BaseDamage to BaseAttackValue) in existing files.

## 2026-05-23 - [Outdated Godot Documentation Discrepancies]
**Observation:** Discovered significant discrepancies between the C# codebase and `WikiCode` documentation. `Movement_System.md` referenced deprecated `BaseSpeed` export variable instead of `Stats.BaseSpeedValue` (managed by `StatManager`), and lacked info on Stun mechanics. `EventBus.md` had outdated implementation details (e.g., didn't explain the `ConcurrentQueue` logic and `WeakAction<T>` direct storage).
**Action:** Updated `Movement_System.md` to explain how `StatManager` controls default speed, and added the `IsStunned` state. Updated `EventBus.md` with accurate code implementation details to explain thread safety and queue execution flow matching `Src/Core/Services/EventBus.cs`.
