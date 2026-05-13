# Ajouter un Archer dans le jeu

## 1. Créer la scène de l'Archer
L'entité "Archer" tire parti du script `EnemyBase` de la même manière que le `Soldier`.

1. Ouvrez le projet Godot.
2. Dupliquez la scène `res://Scenes/NPC/Agressive/Soldier.tscn` et renommez-la en `Archer.tscn`.
3. Détachez le script `Soldier.cs` du nœud racine et attachez-lui plutôt `res://Scenes/NPC/Agressive/Archer.cs`.
4. Dans l'inspecteur, pour le nœud racine `Archer`:
   - Réglez `StoppingDistance` à `200` (cela lui permettra de s'arrêter avant de toucher le joueur pour tirer).
5. Ajustez la taille de détection :
   - Sélectionnez le nœud `DetectionArea` > `CollisionShape2D`.
   - Augmentez le rayon (ex: `300` pixels) afin que l'Archer puisse voir le joueur de plus loin qu'un soldat standard.

## 2. Assurez-vous que le Projectile existe
Le script de l'Archer s'attend à trouver une scène nommée `Arrow.tscn` dans le dossier `res://Scenes/Projectiles/`. Si elle n'existe pas :

1. Créez une nouvelle scène avec comme nœud racine un `Area2D` (renommez-le `Arrow`).
2. Attachez le script `res://Scenes/Projectiles/Arrow.cs`.
3. Ajoutez un `CollisionShape2D` (ex: un petit rectangle).
4. Ajoutez un `Sprite2D` avec la texture de la flèche (assurez-vous que l'image pointe vers la droite, soit 0 degré).
5. Dans `CollisionObject2D` > `Collision` :
   - Assurez-vous que le **Mask** inclut le calque du joueur (Layer 3) pour que la flèche puisse le frapper.
6. Sauvegardez la scène sous `res://Scenes/Projectiles/Arrow.tscn`.

## 3. Comprendre la modulation par niveau
Pour avoir des archers (ou des soldats) plus puissants de différentes couleurs, **il n'est pas nécessaire de créer des scènes séparées** !

Lors de l'instanciation de l'ennemi (via code ou via l'éditeur), modifiez simplement sa propriété exportée `LevelIndex`.
Le code s'occupe de multiplier ses points de vie, ses dégâts, et de modifier automatiquement la couleur du Sprite (`Colors.White` par défaut, `Yellow`, `Red`, `Purple`, `Black`). Plus le niveau est élevé, plus l'ennemi donnera de points au score du joueur lorsqu'il est vaincu.
