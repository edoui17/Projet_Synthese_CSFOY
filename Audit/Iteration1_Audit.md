# 🛠️ MISSION : AUDIT PHASE 1 - STRUCTURE ET AGNOSTICISME

## 🎯 Tâches accomplies :

### 1. **[0.1.1] Audit de la hiérarchie :**
**Analyse :**
Des classes purement axées sur la logique du jeu Godot ou de ses entités se trouvaient dans le dossier `/Src/Core/Domain/Entities/` (ex: `SheepController.cs` et `HealthComponent.cs`), ainsi que leurs interfaces respectives (`IHealthComponent.cs`, `IDamageable.cs`, `INpc.cs`). Étant donné que ces classes et interfaces ne sont utiles que pour le jeu côté client (`IslandSurvivor`) et n'ont pas de valeur pour une base de données, une API ou le Web, elles ont été déplacées selon les conventions requises.

**Actions :**
- Déplacement de `Src/Core/Domain/Entities/SheepController.cs` vers `Src/IslandSurvivor/Logic/Entities/SheepController.cs`.
- Déplacement de `Src/Core/Domain/Entities/HealthComponent.cs` vers `Src/IslandSurvivor/Logic/Entities/HealthComponent.cs`.
- Déplacement de `IHealthComponent.cs`, `IDamageable.cs`, et `INpc.cs` depuis `/Src/Core/Interfaces/Entities/` vers `/Src/IslandSurvivor/Interfaces/`.
- Les namespaces et toutes les références ont été mis à jour de `Core.Domain.Entities` vers `IslandSurvivor.Logic.Entities`, et de `Core.Interfaces` vers `IslandSurvivor.Interfaces` pour garantir que tout compile.
- Le fichier `Tests/UnitTests/Core/Entities/SheepLogicTests.cs` a été déplacé vers `Tests/UnitTests/IslandSurvivor/Logic/SheepLogicTests.cs`.

### 2. **[0.1.2] Agnosticisme du Core :**
**Analyse :**
Après un scan approfondi du projet `/Src/Core` pour détecter les références à Godot, `GodotSharp` ou les types natifs associés, aucune dépendance directe telle que `using Godot;` n'a été trouvée. Toutefois, des composants propres à la logique interne du jeu qui se trouvaient encore dans `/Src/Core` (tel que `SheepController`) utilisaient `System.Numerics.Vector2`.

**Actions :**
- Puisque `SheepController` a été migré vers `IslandSurvivor`, il relève maintenant du domaine du client de jeu. Par conséquent, l'utilisation de `System.Numerics.Vector2` a été remplacée par l'alternative orientée moteur, `Godot.Vector2`, respectant ainsi la directive d'utiliser de préférence les classes propres à Godot pour le code spécifique au client.
- `/Src/Core/` est désormais complètement libre de dépendances liées au jeu spécifique, aux entités ou aux vecteurs de l'UI.

### 3. **[0.1.3] Atomicité (1 classe/fichier) :**
**Analyse :**
Le scan a révélé plusieurs fichiers enfreignant la règle de l'atomicité (1 classe/interface par fichier).

**Actions :**
- **`SheepController.cs`** : Découpé pour extraire la classe statique `SheepStates` vers son propre fichier `Src/IslandSurvivor/Logic/Entities/SheepStates.cs`.
- **`SheepLogicTests.cs`** : Séparé en `HealthComponentTests.cs` et `SheepControllerTests.cs` situés dans le dossier `Tests/UnitTests/IslandSurvivor/Logic/`.
- **`IScoreTracker.cs`** : La classe imbriquée publique `ScoreChangedEventArgs` a été extraite vers `Src/Core/Interfaces/Stats/ScoreChangedEventArgs.cs`. Les implémentations liées comme `ScoreTracker.cs` et `ScoreManager.cs` ont été adaptées.
- **Note sur les classes internes/privées** : Les sous-classes privées ou les définitions d'événements fortement imbriquées qui sont intrinsèquement liées à une classe mère (telles que les `EventArgs` dans `ISignalManager` ou `WeakDelegate` dans `WeakEvent`) ont été conservées conformément aux bonnes pratiques C# en matière d'encapsulation.

**Résultat :**
Toutes les compilations se déroulent avec succès après ces modifications.
