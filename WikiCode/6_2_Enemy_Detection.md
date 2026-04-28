# 6.2 - Détection et Poursuite de l'Ennemi (Line of Sight)

Ce document explique l'implémentation du système de détection des ennemis, permettant au `Soldier` de repérer le joueur dans un rayon et de vérifier la ligne de vue avant de passer à l'attaque (poursuite).

## Architecture

Le système utilise trois composants principaux :
1. **Area2D (`DetectionArea`)** : Permet de définir le rayon maximal de vision de l'ennemi.
2. **RayCast2D (`LineOfSightRay`)** : Permet de tracer un rayon entre l'ennemi et le joueur pour s'assurer qu'aucun mur ne bloque la vue.
3. **AgressorController** : La logique pure C# dans la couche Core/Logic, gérant les états `IDLE`, `CHASE` et le timer de désengagement.

## Configuration Godot (Nœuds et Signaux)

Pour que la détection fonctionne dans la scène `Soldier.tscn` :

1. **DetectionArea (Area2D)** :
   - Ajoutez un nœud `Area2D` nommé `DetectionArea` en enfant de `Soldier`.
   - Ajoutez-lui une `CollisionShape2D` (ex: CircleShape2D de rayon 250px).
   - **Important** : Le masque de collision (Collision Mask) de ce Area2D doit pouvoir détecter la couche (Layer) sur laquelle se trouve le `Player` (ex: Mask = 3).
   - Les signaux `body_entered` et `body_exited` sont connectés par code dans `Soldier.cs` (pas besoin de le faire dans l'éditeur).

2. **LineOfSightRay (RayCast2D)** :
   - Ajoutez un nœud `RayCast2D` nommé `LineOfSightRay` en enfant de `Soldier`.
   - **Exclude Parent** : Cochez cette case pour éviter que le rayon ne percute le `Soldier` lui-même.
   - **Collision Mask** : Configurez le masque pour qu'il détecte le décor (ex: Mask = 1) et le joueur (ex: Mask = 3). Le script gérera le résultat pour savoir s'il s'agit d'un mur ou du joueur.
   - Assurez-vous qu'**aucun script** (comme `AgressorController.cs`) n'est attaché à ce nœud dans l'éditeur.

3. **Le Joueur (Player)** :
   - Assurez-vous que le nœud racine de la scène du joueur s'appelle `Player` ou qu'il fait partie du groupe `Player` (onglet Nœud > Groupes).

## Logique de Détection (Soldier.cs)

### 1. Entrée dans la zone
Lorsque le joueur entre dans le rayon (`DetectionArea`), l'événement `OnDetectionAreaBodyEntered` enregistre le joueur comme cible potentielle.
```csharp
if (p_body.IsInGroup("Player") || p_body.Name == "Player") { m_targetPlayer = p_body; }
```

### 2. Vérification du Rayon (Line of Sight)
À chaque frame (dans `_PhysicsProcess`), la méthode `CheckLineOfSight()` pointe le `RayCast2D` vers le joueur :
- Si le rayon touche un nœud qui n'est pas le joueur (ex: un mur), la méthode retourne `false` (vue bloquée).
- Si le rayon touche le joueur, ou ne touche rien (cas où le mur est esquivé), la méthode retourne `true` (vue dégagée).

### 3. Changement d'État
Le `AgressorController` reçoit ce booléen.
- Si le joueur est en vue : il passe en `CHASE` et réinitialise son timer de désengagement.
- Si le joueur disparaît (derrière un mur) : l'état reste `CHASE` brièvement pendant que le timer s'écoule, puis retombe en `IDLE` (errance).
