## Session Lifecycle Management
Discovered that without a dedicated Godot `WorldManager`, `Autoload`s effectively serve as lifecycle hooks. By attaching a single `SessionManager` autoload solely dedicated to listening for `SessionEnded`, we can isolate state-reset behavior away from the Main Menu, ensuring that navigation between menus doesn't inadvertently wipe state unless explicitly broadcasted.

Additionally, when implementing `ResetStats` on `StatTracker`, it's critical to restore `Health` specifically to its `EffectiveMaxValue` (using `SetCurrentValue`) rather than `0`, while other volatile stats (Speed, Attack modifiers) revert to `0`.

## Session Lifecycle Management
Discovered that without a dedicated Godot `WorldManager`, `Autoload`s effectively serve as lifecycle hooks. By attaching a single `SessionManager` autoload solely dedicated to listening for `SessionEnded`, we can isolate state-reset behavior away from the Main Menu, ensuring that navigation between menus doesn't inadvertently wipe state unless explicitly broadcasted.

Additionally, when implementing `ResetStats` on `StatTracker`, it's critical to restore `Health` specifically to its `EffectiveMaxValue` (using `SetCurrentValue`) rather than `0`, while other volatile stats (Speed, Attack modifiers) revert to `0`.
