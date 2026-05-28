## 2025-05-15 - [Refactoring] Flattened Nested Ifs with Guard Clauses
**Observation:** Deeply nested if statements were present in StatManager _ValidateProperty method, increasing cognitive load.
**Action:** Applied guard clauses (early returns) to flatten nesting and improve readability according to Clean Code conventions.
## 2025-05-28 - [Refactoring] Flattened Nested Ifs in State Updates
**Observation:** Deeply nested if statements were present in State classes (specifically DashState `PhysicsUpdate` method), often related to collision checks and handling, increasing cognitive load and hindering readability.
**Action:** Applied guard clauses (early returns) by extracting collision handling and logic into separate helper methods to flatten nesting and improve readability according to Clean Code conventions. Ensured field names and parameter names follow standard conventions.
