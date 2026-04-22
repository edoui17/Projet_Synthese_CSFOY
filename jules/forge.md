
## 2026-04-22 - Navigation Architecture Updates
- Updated the `NavigationService` to perform synchronous validation of resources before dispatching `NavigationRequestedEvent`.
- Introduced `IShopManager` and `IInventoryManager` dependencies directly into `NavigationService` to act as a secure gateway for navigation purchases without coupling the Godot `NavigationMenu` directly to shop logic.
- Demonstrated proper integration of Godot UI state (disabling buttons) based on purely Core-managed state.
