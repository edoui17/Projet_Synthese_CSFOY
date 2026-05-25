## 2025-05-15 - [Refactoring] Flattened Nested Ifs with Guard Clauses
**Observation:** Deeply nested if statements were present in StatManager _ValidateProperty method, increasing cognitive load.
**Action:** Applied guard clauses (early returns) to flatten nesting and improve readability according to Clean Code conventions.
## 2026-05-25 - [Private Field Naming Anti-Pattern]
**Observation:** Discovered a recurring anti-pattern where private fields are named using standard C# `_camelCase` instead of the project-mandated `m_camelCase` as defined in `Convention.md` (e.g., `_hasDealtDashDamage` in `DashState.cs`, `_level` in `HealthBarLvl.cs`).
**Action:** All new private fields must strictly use the `m_camelCase` format to ensure compliance with the `Convention.md` standard. Existing instances should be refactored when touching those files.
