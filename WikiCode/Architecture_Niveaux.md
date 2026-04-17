# Architecture des Niveaux (LevelBase)

Ce document décrit comment l'architecture des scènes (niveaux) est structurée dans **IslandSurvivor**, basée sur l'héritage de scènes Godot pour respecter les principes DRY (Don't Repeat Yourself).

## 1. La Scène de Base (`LevelBase.tscn`)

Pour éviter de dupliquer les nœuds globaux dans chaque nouvelle île (comme l'interface utilisateur ou le joueur), une scène racine `LevelBase.tscn` a été créée. Toutes les îles jouables **doivent hériter** de cette scène.

### Structure de `LevelBase.tscn`
*   `MapContainer` (Node2D) : Un conteneur vide destiné à recevoir la carte (TileMaps) spécifique au niveau.
*   `Player` (PackedScene) : L'entité du joueur préconfigurée avec ses composants de santé, statistiques et gestionnaire d'interaction.
*   `ScoreManager` (PackedScene) : Le pont (Bridge) gérant le système de score persistant de ce niveau.
*   `CanvasLayer` :
    *   `PlayerHUD` : L'interface de base (Barre de vie, score, etc.).
    *   `NavigationMenu` : Le planificateur d'îles (désactivé/invisible par défaut).
    *   `GameMaterialControl` : Le menu de la boutique/hub.

> **Note de Game Design :** L'interface globale est attachée à un `CanvasLayer` dans la `LevelBase.tscn` (Screen Space) afin d'ignorer le zoom ou le mouvement de la caméra, garantissant que les clics de souris sont toujours captés correctement par l'UI.

## 2. Création d'un nouveau niveau (Héritage)

Lors de la création d'une nouvelle île (ex: `PlayerHub.tscn` ou `Level1.tscn`), il ne faut **pas** repartir de zéro.

1.  Dans Godot, faites *Scene > New Inherited Scene* (Nouvelle Scène Héritée).
2.  Sélectionnez `Src/IslandSurvivor/Scenes/Level/LevelBase.tscn`.
3.  Renommez la racine `Main`.

### Injection de la Carte
Dans votre nouvelle scène héritée, vous devez ajouter la carte visuelle (qui contient vos TileMaps, collisions de l'eau, etc.).
*   Instanciez votre scène de carte (`base_map_island.tscn` ou autre).
*   Glissez cette carte **à l'intérieur** du nœud `MapContainer`.
*   Assurez-vous qu'elle est à l'index 0 (au-dessus du joueur dans l'arbre) pour que le personnage soit rendu par-dessus l'environnement.

### Ajout d'un Portail de sortie
Chaque niveau doit avoir un moyen d'en sortir (navigation vers une autre île).
*   Instanciez une scène `Portal.tscn`.
*   Placez-le à la racine de la scène (au même niveau que le `Player`).
*   Positionnez le portail à l'endroit désiré sur votre carte.
*   **Comportement automatique** : Si la scène n'est pas le `PlayerHub`, le `Portal.cs` s'activera automatiquement au chargement pour pointer vers l'île principale (`HomeIsland`), garantissant toujours un chemin de retour.

## 3. Avantages de cette structure

*   **Maintenance Centralisée** : Si nous décidons d'ajouter un menu de pause global ou de modifier la caméra du joueur, il suffit de modifier `LevelBase.tscn`. Tous les niveaux (`Level1` à `Level5` et `PlayerHub`) hériteront instantanément de cette amélioration.
*   **Indépendance** : Le système de navigation UI (qui a besoin du joueur et du portail) fonctionnera toujours car il sait qu'il peut chercher `Main/Player` et `Main/Portal` peu importe l'île chargée.
