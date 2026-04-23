
## 2026-04-23: Pub-Sub Bridge Refactor
- Eliminated hybrid `WeakEvent` bridging logic in Core managers in favor of pure `IEvent` payloads published to the `EventBus`.
- `SignalManager` is now exclusively a Godot-side Autoload translator. It listens to Godot Signals and publishes `IEvent`s, and subscribes to `IEvent`s to emit Godot Signals for UI synchronization.
- **Godot Quirk**: Godot signals don't handle C# custom objects well, so complex Core events (`IEvent`) are decomposed into primitive types (int, string) before being emitted as native signals by the `SignalManager`.
