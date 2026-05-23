## 2024-10-25 - [Cache Node Lookups in Hot Paths]
**Learning:** `GetNodeOrNull<T>()` and `GetNode<T>()` create string marshaling overhead and perform tree lookups which generate minor GC pressure and unnecessary CPU usage. When called multiple times per physics frame (e.g. inside `_PhysicsProcess` or `UpdateAnimation` in `EnemyBase`), this compounds into measurable frame time spikes, especially with many enemies active simultaneously.
**Action:** Always cache frequently accessed Node references inside a class field during `_Ready()` rather than polling them dynamically in `_Process` or `_PhysicsProcess`.

## 2024-10-25 - [Optimize StringName and ToString() in Hot Paths]
**Learning:** Calling `.ToString()` on a Godot `StringName` object (like `AnimatedSprite2D.Animation`) or implicitly passing a literal string to a Godot API expecting a `StringName` (like `Play()`) allocates a new C# string on the heap. When done in hot paths such as `_PhysicsProcess`, this results in noticeable Garbage Collection micro-stutters and overhead.
**Action:** Pre-cache strings as `StringName` instances in fields (e.g. `m_animAttack = new StringName("Attack");`), check state conditions to avoid redundant engine calls, and explicitly check if debug state values have changed before allocating `.ToString()` formatting updates.

## 2024-10-25 - [Throttle UI Updates in Dash Timer]
**Learning:** Formatting strings with interpolation (`$"Dash: {...}s"`) inside `_PhysicsProcess` allocates heap memory every frame. Even if the visual number on screen doesn't change every frame (e.g., updating every 0.1s), the engine is allocating a new string every frame, causing minor GC stutters.
**Action:** Always track the discrete integer value (e.g. deciseconds) and only re-format/assign the `Label.Text` property if that tracking value has changed compared to the previous frame.
## 2024-05-22 - Replacing `DistanceTo` with `DistanceSquaredTo` in Godot Hot Paths
**Learning:** In Godot C# (and game engines generally), distance checks using `DistanceTo` require computing a square root, which is a relatively expensive operation. When performed inside hot paths like `_PhysicsProcess` across multiple aggressive NPC entities concurrently chasing the player, these CPU cycles add up and can contribute to micro-stutters or frame time variations.
**Action:** When evaluating distance thresholds (like checking if an NPC is within `StoppingDistance` or `MeleeDistance`), cache the squared distance target (e.g., `MeleeDistance * MeleeDistance`) and compare it against `GlobalPosition.DistanceSquaredTo(target.GlobalPosition)`. Always use `DistanceSquaredTo` in high-frequency loops.
