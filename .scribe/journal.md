## 2026-05-20 - [Missing Core Architectural Guide]
**Observation:** The 'Accountant vs. Orchestrator' architectural boundary is a core pillar of the project (mentioned in Convention.md and forges.md), but lacks a dedicated master reference in the WikiCode folder to explain the *why* and *how* to new developers.
**Action:** Created `WikiCode/Accountant_vs_Orchestrator.md` to serve as the master guide, standardizing how we document the separation between pure C# Core logic and Godot Client nodes, and updated outdated terminology (e.g., BaseDamage to BaseAttackValue) in existing files.

## 2026-05-28 - [Deprecated AgressorController Documentation]
**Observation:** Discovered a major discrepancy where `WikiCode/combat_system.md` and `WikiCode/Systeme_Ennemis_Deplacement.md` still document the deprecated pure C# `IAgressorController` and `AnimatedSprite2D` architecture for enemies, rather than the new Node-based StateMachine composition pattern and `AnimationPlayer`.
**Action:** Updated `combat_system.md` and `Systeme_Ennemis_Deplacement.md` to accurately reflect the Orchestrator Node-based StateMachine architecture (`StateMachine`, `ChaseState`, `MeleeAttackState`, etc.) and the use of `Sprite2D` with `AnimationPlayer`, ensuring alignment with `NPC_StateMachine_Architecture.md`.
