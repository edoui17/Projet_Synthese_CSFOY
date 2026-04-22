# Technical Learnings and Architectural Decisions

## 2026-04-22 - EventBus Architecture Finalization
- **Decision**: Implemented an explicit Pub-Sub `EventBus` in `Src/Core/Services` rather than `Src/Core/Managers`.
- **Reasoning**: The `EventBus` is an infrastructural, cross-cutting concern rather than game state logic, hence it belongs in `Services` to maintain N-Tier strictness.
- **Godot/C# Quirks**: Relying entirely on `WeakReference` for cleanup in Godot C# nodes can be non-deterministic due to Garbage Collector timing. To solve this, `Unsubscribe` was explicitly added to the `IEventBus` interface and mandated for short-lived nodes (e.g., UI menus) to prevent them from responding to events after they are hidden but before GC collects them.
