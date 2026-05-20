## 2026-05-20 - [Missing Core Architectural Guide]
**Observation:** The 'Accountant vs. Orchestrator' architectural boundary is a core pillar of the project (mentioned in Convention.md and forges.md), but lacks a dedicated master reference in the WikiCode folder to explain the *why* and *how* to new developers.
**Action:** Created `WikiCode/Accountant_vs_Orchestrator.md` to serve as the master guide, standardizing how we document the separation between pure C# Core logic and Godot Client nodes, and updated outdated terminology (e.g., BaseDamage to BaseAttackValue) in existing files.
