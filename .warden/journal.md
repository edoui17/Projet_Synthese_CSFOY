## 2025-05-15 - [Refactoring] Flattened Nested Ifs with Guard Clauses
**Observation:** Deeply nested if statements were present in StatManager _ValidateProperty method, increasing cognitive load.
**Action:** Applied guard clauses (early returns) to flatten nesting and improve readability according to Clean Code conventions.
