1. **Add `EntityType` enum to Core**
   - Create `Src/Core/Enums/EntityType.cs` with values `Player` (0), `NPC` (1), and `Resource` (2).
2. **Update `StatManager.cs`**
   - Add `[Export] private EntityType m_entityType = EntityType.NPC;` (importing `Core.Enums`).
   - Modify `_Ready()` so that the `initialStats` dictionary only includes stats relevant to the `m_entityType`:
     - **Resource**: Only `Health`.
     - **NPC**: `Health`, `Attack`, `Speed`.
     - **Player**: `Health`, `Attack`, `Speed`, `Luck`.
3. **Update Godot `.tscn` files**
   - Set the `m_entityType` for all the entities (Player = 0, Sheep/Soldier = 1, Gold/Rock/Trees = 2).
4. **Log AI Usage**
   - Update `jules/ai_usage_kh.md` with the new changes in the specified format.
5. **Pre-commit Steps**
   - Complete pre-commit checks to ensure proper testing, verification, review, and reflections.
6. **Submit**
   - Submit the change with a descriptive commit message.
