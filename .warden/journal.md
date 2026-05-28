## 2026-05-15 - [Refactoring] Flattened Nested Ifs with Guard Clauses
**Observation:** Deeply nested if statements were present in StatManager _ValidateProperty method, increasing cognitive load.
**Action:** Applied guard clauses (early returns) to flatten nesting and improve readability according to Clean Code conventions.
## 2026-05-25 - [Private Field Naming Anti-Pattern]
**Observation:** Discovered a recurring anti-pattern where private fields are named using standard C# `_camelCase` instead of the project-mandated `m_camelCase` as defined in `Convention.md` (e.g., `_hasDealtDashDamage` in `DashState.cs`, `_level` in `HealthBarLvl.cs`).
**Action:** All new private fields must strictly use the `m_camelCase` format to ensure compliance with the `Convention.md` standard. Existing instances should be refactored when touching those files.
## 2026-05-26 - [Convention] Private Field Naming Violations (_ vs m_)
**Observation:** Discovered recurring anti-pattern across UI scripts (e.g., `MaterialsMenuPlanner.cs`, `BuildingNode.cs`) where private fields were prefixed with `_` instead of the mandated `m_` prefix.
**Action:** The team must strictly adhere to the `m_camelCase` standard for private fields as dictated by `Convention.md` to distinguish them from local variables or Godot Node parameters.
## 2026-05-27 - Extracting Magic Strings and Flattening Collision Logic
**Observation:** BaseProjectile contained hardcoded group and node strings (e.g., "Player", "EnnemiesNPC") and deeply nested if-statements inside the HandleCollision logic, which made it harder to read and prone to typos.
**Action:** Centralized these magic strings into `const string` fields using UPPER_SNAKE_CASE (e.g. `GROUP_PLAYER`), and flattened the target-hit logic using early returns / guard clauses (`if (!IsValidTarget(p_node)) return;`), enforcing the Open/Closed boundary for adding new factions cleanly.
## 2026-05-28 - [Refactoring] Flattened Nested Ifs in State Updates
**Observation:** Deeply nested if statements were present in State classes (specifically DashState `PhysicsUpdate` method), often related to collision checks and handling, increasing cognitive load and hindering readability.
**Action:** Applied guard clauses (early returns) by extracting collision handling and logic into separate helper methods to flatten nesting and improve readability according to Clean Code conventions. Ensured field names and parameter names follow standard conventions.
