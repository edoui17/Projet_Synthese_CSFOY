## 2024-10-25 - [Cache Node Lookups in Hot Paths]
**Learning:** `GetNodeOrNull<T>()` and `GetNode<T>()` create string marshaling overhead and perform tree lookups which generate minor GC pressure and unnecessary CPU usage. When called multiple times per physics frame (e.g. inside `_PhysicsProcess` or `UpdateAnimation` in `EnemyBase`), this compounds into measurable frame time spikes, especially with many enemies active simultaneously.
**Action:** Always cache frequently accessed Node references inside a class field during `_Ready()` rather than polling them dynamically in `_Process` or `_PhysicsProcess`.

## 2024-10-25 - [Optimize StringName and ToString() in Hot Paths]
**Learning:** Calling `.ToString()` on a Godot `StringName` object (like `AnimatedSprite2D.Animation`) or implicitly passing a literal string to a Godot API expecting a `StringName` (like `Play()`) allocates a new C# string on the heap. When done in hot paths such as `_PhysicsProcess`, this results in noticeable Garbage Collection micro-stutters and overhead.
**Action:** Pre-cache strings as `StringName` instances in fields (e.g. `m_animAttack = new StringName("Attack");`), check state conditions to avoid redundant engine calls, and explicitly check if debug state values have changed before allocating `.ToString()` formatting updates.
