# AI Usage Journal

### 2023-10-27 - [User Story 5.2 : Implémenter le système de point]
| **Request** | **AI Contribution** | **Decision Reasoning** |
| :--- | :--- | :--- |
| Implement Score system according to N-Tier Architecture, creating a SessionState in Core, a SessionResource in Godot, ScoreTracker with HighScore saving, and bridging via ScoreManager. | I created the complete feature suite including the `SessionState` for domain logic, `ISaveService` & `GodotSaveService` for cross-platform data persistence, `ScoreTracker` in Core for logic and validation, `SessionResource` as a Godot template, and `ScoreManager` as the Bridge emitting native Godot signals. Unit tests with Moq integration were also developed. | A strict separation of state (Core) and template (Godot Resource) was used to ensure testability and prevent mutation of Godot resources during runtime. `GodotSaveService` was injected into the Core to keep the Core entirely agnostic of the Godot engine's file system structure. WeakEvents ensure proper lifecycle management and avoid memory leaks. |
