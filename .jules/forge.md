# Technical Learnings and Architectural Decisions

## 2026-04-22 - EventBus Architecture Finalization
- **Decision**: Implemented an explicit Pub-Sub `EventBus` in `Src/Core/Services` rather than `Src/Core/Managers`.
- **Reasoning**: The `EventBus` is an infrastructural, cross-cutting concern rather than game state logic, hence it belongs in `Services` to maintain N-Tier strictness.
- **Godot/C# Quirks**: Relying entirely on `WeakReference` for cleanup in Godot C# nodes can be non-deterministic due to Garbage Collector timing. To solve this, `Unsubscribe` was explicitly added to the `IEventBus` interface and mandated for short-lived nodes (e.g., UI menus) to prevent them from responding to events after they are hidden but before GC collects them.

## 2026-04-22 - Phase 3 EDA Migration
- **Decision**: Refactored Core Managers to an Event-Driven Architecture (EDA) to break direct tight couplings (e.g., `NavigationService` no longer injects `IInventoryManager`). Instead, they communicate exclusively via POCOs in `Src/Core/Events/`.
- **Decision**: EventBus upgraded to use an Event Queue (Option B) using a `ConcurrentQueue<Action>` and a `ProcessEvents()` method. This prevents recursive event chains, prevents synchronous UI blocking during heavy event dispatch, and prepares the core for async database tracking.
- **Godot/C# Quirks**: `WeakAction<T>` was correctly decoupled to `Src/Core/Utils/WeakAction.cs` to maintain architecture purity, treating it like a standard primitive utility instead of a hidden inner class inside the EventBus.
