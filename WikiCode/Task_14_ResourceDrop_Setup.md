# Configuration de la Scène ResourceDrop (Fix Visuel des Ressources)

Suite à la correction des chemins d'images (`IconPath`) dans le code C# pour que les ressources (Or, Roche, Bois, Viande) affichent la bonne icône lors de leur destruction, vous devez créer manuellement la scène `ResourceDrop.tscn` dans l'éditeur Godot pour que l'animation Tween fonctionne.

Le code tente de charger cette scène au chemin strict : `res://Scenes/Ressources/ResourceDrop.tscn`. Si elle n'existe pas, les ressources ne feront pas l'animation de chute/tween visuelle.

## Étapes de création dans l'éditeur Godot :

1.  **Ouvrir l'éditeur Godot** et ouvrir le projet `IslandSurvivor`.
2.  Allez dans le menu **Scene > New Scene** (Scène > Nouvelle Scène).
3.  Sélectionnez **2D Scene** (ou Node2D) comme nœud racine.
4.  **Renommez** le nœud racine en `ResourceDrop`.
5.  Dans le panneau *FileSystem*, naviguez jusqu'au script : `Src/IslandSurvivor/Scenes/Ressources/ResourceDrop.cs`.
6.  **Glissez-déposez** ce script `ResourceDrop.cs` sur le nœud racine `ResourceDrop` pour l'attacher.
7.  **(Important)** Sauvegardez la scène via `Ctrl+S` (ou `Cmd+S`).
8.  Enregistrez le fichier **exactement** à ce chemin dans votre projet :
    `Src/IslandSurvivor/Scenes/Ressources/ResourceDrop.tscn`
    *(Ce qui correspond à `res://Scenes/Ressources/ResourceDrop.tscn` du point de vue de Godot).*

**C'est tout !**
Vous n'avez pas besoin d'ajouter de `Sprite2D` enfant manuellement à cette scène. Le script `ResourceDrop.cs` se charge de créer le `Sprite2D` par code au moment de l'exécution (`_Ready`) et lui assignera l'icône de la ressource correspondante avec les bons chemins que nous venons de corriger (Or, Roche, Viande, etc.).

Une fois cette scène créée, détruire un arbre, une roche, de l'or ou un mouton devrait instantanément générer plusieurs petites icônes qui sautent légèrement puis volent vers le joueur (Tween).
