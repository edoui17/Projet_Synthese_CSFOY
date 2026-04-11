
### 2026-04-11 - WeakEvent Subscription in Core Bridge
- **Discovery**: Custom `WeakEvent` implementation in the Core uses `.AddListener()` and `.RemoveListener()` instead of the standard `+=` and `-=` operators or `.Subscribe()` / `.Unsubscribe()`. Always verify the specific custom utilities provided by the Core before trying to attach standard C# event handler logic.

### 2026-04-11 - UI Placement in Godot 4.6.1
- **Discovery**: When placing `Control` nodes directly inside a `Node2D` hierarchy (world space), they correctly follow the world coordinates and are affected by the camera. Wrapping them in a `CanvasLayer` detaches them from world coordinates and pins them to screen space.
- **Quirk**: Input events for UI elements (like `Button` clicks) can be absorbed or ignored if the UI is deep in the game world tree and obscured by collision/mouse filters of sibling `Node2D`/`Area2D`s. Placing global interactive menus (like `NavigationMenu`) into the main scene's `CanvasLayer` guarantees they are at the forefront of the viewport and reliably intercept mouse events.
