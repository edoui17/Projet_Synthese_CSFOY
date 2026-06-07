### 2024-06-07 - [Refactoring de l'AttackState (Architecture Hiérarchique)]
**Request**: Refactoriser la structure des états d'attaque pour introduire une classe de base `AttackState` avec sous-états `MeleeAttackState`, `RangedAttackState`, `MagicAttackState` et intégrer le `GuardingState` tout en centralisant la décision dans `AggressiveNpcBase`.
**AI Contribution**:
1. Created `AttackState.cs` inheriting from `State` to centralize duplicated attack target orientation and configuration.
2. Refactored `MeleeAttackState`, `RangedAttackState`, and `MagicAttackState` to inherit from `AttackState`.
3. Renamed `GuardState.cs` to `GuardingState.cs`, updated it to face the player, and correctly exit when out of range.
4. Centralized `GetCombatDecisionState()` in `AggressiveNpcBase.cs` to dynamically evaluate states based on `m_stateMachine.HasState` capabilities, deprecating overrides in child NPCs.
5. Removed redundant implementations from `MeleeAggressiveNpcBase`, `RangedAggressiveNpcBase`, `Lancer`, and `Reaper`.
**Decision Reasoning**: Generalization forces clean separation of concerns where child nodes determine capabilities instead of hardcoded C# routing. Implementing states strictly correctly as Nodes enables designers to construct bosses freely by adding states to the StateMachine tree without rewriting routing code.

### 2024-06-07 - [Refactoring de l'AttackState (Architecture Hiérarchique)]
**Request**: Refactoriser la structure des états d'attaque pour introduire une classe de base `AttackState` avec sous-états `MeleeAttackState`, `RangedAttackState`, `MagicAttackState` et intégrer le `GuardingState` tout en centralisant la décision dans `AggressiveNpcBase`.
**AI Contribution**:
1. Created `AttackState.cs` inheriting from `State` to centralize duplicated attack target orientation and configuration.
2. Refactored `MeleeAttackState`, `RangedAttackState`, and `MagicAttackState` to inherit from `AttackState`.
3. Renamed `GuardState.cs` to `GuardingState.cs`, updated it to face the player, and correctly exit when out of range.
4. Centralized `GetCombatDecisionState()` in `AggressiveNpcBase.cs` to dynamically evaluate states based on `m_stateMachine.HasState` capabilities, deprecating overrides in child NPCs.
5. Removed redundant implementations from `MeleeAggressiveNpcBase`, `RangedAggressiveNpcBase`, `Lancer`, and `Reaper`.
**Decision Reasoning**: Generalization forces clean separation of concerns where child nodes determine capabilities instead of hardcoded C# routing. Implementing states strictly correctly as Nodes enables designers to construct bosses freely by adding states to the StateMachine tree without rewriting routing code.
