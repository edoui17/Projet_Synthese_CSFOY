## 2025-05-15 - [Refactoring] Flattened Nested Ifs with Guard Clauses
**Observation:** Deeply nested if statements were present in StatManager _ValidateProperty method, increasing cognitive load.
**Action:** Applied guard clauses (early returns) to flatten nesting and improve readability according to Clean Code conventions.

## 2025-05-26 - [Convention] Private Field Naming Violations (_ vs m_)
**Observation:** Discovered recurring anti-pattern across UI scripts (e.g., `MaterialsMenuPlanner.cs`, `BuildingNode.cs`) where private fields were prefixed with `_` instead of the mandated `m_` prefix.
**Action:** The team must strictly adhere to the `m_camelCase` standard for private fields as dictated by `Convention.md` to distinguish them from local variables or Godot Node parameters.
