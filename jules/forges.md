## 2024-06-07 - Refactored Npc Combat State Management
**Discovery**: Combat decision states were deeply hardcoded and duplicated across subclasses of `AggressiveNpcBase`.
**Action**: Promoted a central decision-making process into `AggressiveNpcBase.cs`. It now iterates over `m_stateMachine.HasState` allowing Godot editor-driven design. Extracted shared orientation calculation into a new `AttackState` base class. Migrated `GuardState` to a true continuous tracking `GuardingState`.

## 2024-06-07 - Refactored Npc Combat State Management
**Discovery**: Combat decision states were deeply hardcoded and duplicated across subclasses of `AggressiveNpcBase`.
**Action**: Promoted a central decision-making process into `AggressiveNpcBase.cs`. It now iterates over `m_stateMachine.HasState` allowing Godot editor-driven design. Extracted shared orientation calculation into a new `AttackState` base class. Migrated `GuardState` to a true continuous tracking `GuardingState`.
