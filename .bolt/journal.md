## 2024-05-20 - [AnimationPlayer Track Order Execution Bug]
 **Learning:** Godot evaluates Animation tracks sequentially based on index. When swapping grid sizes during animations, placing the `frame` track before `vframes` and `hframes` tracks can cause 'set_frame: Index p_frame is out of bounds' errors, breaking playback state and stalling code dependent on `AnimationFinished` signals.
 **Action:** Reorder tracks in the `.tscn` file so that `texture`, `hframes`, and `vframes` evaluate before the `frame` track.

## 2026-05-28 - [GetNode String Allocation in State Loops]
 **Learning:** Using `GetNodeOrNull<T>()` inside state update loops (like `Update` or `PhysicsUpdate` in `IdleState` and `ChaseState`) causes continuous string marshaling and CLR allocations, producing unnecessary GC spikes in Godot 4 C#.
 **Action:** Always cache node references in private fields during a state's `Initialize()` method to avoid structural lookup overhead in physics or process hot paths.
## 2024-05-30 - [DistanceSquaredTo over DistanceTo]
 **Learning:** Godot's `DistanceTo` performs an expensive square root operation, which adds up during heavy looping, such as evaluating spawn locations multiple times per frame.
 **Action:** Prefer `DistanceSquaredTo` when comparing distances, especially in loops and physics frames. Manually square the threshold distance for comparison.
## 2026-05-30 - [DistanceSquaredTo over DistanceTo everywhere]
 **Learning:** In C#/Godot, systematically changing `DistanceTo` to `DistanceSquaredTo` across all distance calculations (e.g. evaluating interactables distances per-frame or close to it) prevents expensive square root operations and marginally reduces runtime costs.
 **Action:** Replaced `DistanceTo` with `DistanceSquaredTo` across the `IInteractable` implementers and `InteractionService`.

## 2026-05-31 - [O(N²) Godot API marshaling anti-pattern]
 **Learning:** Iterating over Godot array/count API like `GetSlideCollisionCount()` and subsequently re-calling it in nested logic inside `_PhysicsProcess` or state `PhysicsUpdate` methods creates O(N²) marshaling overhead between C# and C++.
 **Action:** Always cache Godot count or lookup properties into local variables before entering loops inside hot paths to avoid redundant engine barrier crossings.
