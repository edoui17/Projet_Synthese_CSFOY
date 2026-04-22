
## Dependency Injection and EventBus

*   **Initialization:** In an N-Tier architecture integrating with Godot, a central Autoload (`ServiceRegistry`) is the optimal location to instantiate cross-cutting infrastructure like the `EventBus` (`Core.Services.EventBus`).
*   **Injection:** The `EventBus` instance should be passed to Core Managers via constructor injection during `ServiceRegistry._EnterTree()`.
*   **Processing:** Since the custom `EventBus` implementation uses a deferred queue, the Autoload must override `_Process(double delta)` to call `EventBus.ProcessEvents()` every frame, ensuring the queue is actively drained.
*   **Unit Tests & Namespaces:** When modifying Core Manager constructors to accept an `IEventBus` parameter, update corresponding unit tests. Use `global::Core.Services.EventBus` instead of just `Core.Services.EventBus` to prevent namespace collision errors (e.g., MSBuild mistaking it for `UnitTests.Core.Services`).
