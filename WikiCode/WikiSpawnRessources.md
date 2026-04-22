# Documentation : Système de Spawn Aléatoire de Ressources (ResourceZone)

## Vue d'ensemble
Le système `ResourceZone` permet de peupler dynamiquement des zones spécifiques de la carte avec des ressources (arbres, rochers, mines d'or, etc.). Il utilise une zone polygonale pour définir l'aire de répartition et gère automatiquement le cycle de vie des ressources (spawn, destruction, respawn).

## Architecture
Le système suit une architecture N-Tier avec une approche "Interface First".

### 1. Core (Logique Pure)
- **`IResourcePopulator`** : Interface définissant la capacité à peupler une zone avec des ressources.

### 2. IslandSurvivor (Godot)
- **`ResourceZone` (Node : Node2D)** : Définit une zone de peuplement.
    - Utilise un enfant **`Polygon2D`** (nommé "SpawningArea") pour définir la forme de la zone.
    - Gère une **Safe Zone** (Rayon et Centre) pour éviter le spawn sur le joueur.
    - Utilise une liste de **`ResourceScenes`** configurables via l'inspecteur (Drag & Drop).
    - Vérifie la validité du spawn (pas dans l'eau, distance minimale entre ressources).
    - Gère le **Respawn** automatique via un timer configurable.
    - Force la couche de collision (Layer 5 : Ressource).

## Paramètres Configurables

| Paramètre | Description | Défaut |
| :--- | :--- | :--- |
| `ResourceCount` | Nombre maximum de ressources dans la zone. | 10 |
| `ResourceScenes` | Liste de scènes `.tscn` à spawn (Drag & Drop depuis l'éditeur). | [] |
| `SafeZoneRadius` | Distance minimale du centre de sécurité. | 150f |
| `SafeZoneCenter` | Position locale du centre de sécurité. | (0, 0) |
| `MinDistanceBetweenResources` | Distance minimale entre deux ressources. | 50f |
| `RespawnInterval` | Temps (sec) entre les tentatives de respawn. | 30f |
| `WaterTileMap` | Référence au TileMapLayer d'eau pour validation. | null |

## Contraintes et Validations
1. **Exclusions** : Le système ne doit pas être utilisé pour `RessourceForBaseMap` qui possède sa propre logique.
2. **Eau** : Si `WaterTileMap` est assigné, aucune ressource ne peut spawn sur une tuile d'eau.
3. **Distance** : Évite la superposition des ressources en respectant `MinDistanceBetweenResources`.
4. **Physique** : Les ressources spawnées sont automatiquement assignées au Layer 5 (Collision Bit 16).

## Utilisation

### Créer une zone de ressources
1. Créez un node `Node2D` et attachez-lui le script `ResourceZone.cs`.
2. Ajoutez un enfant `Polygon2D` nommé **"SpawningArea"**.
3. Dessinez la forme de la zone.
4. Dans l'inspecteur, ajoutez des éléments à la liste **`ResourceScenes`** en y glissant des scènes de ressources (ex: `Rock.tscn`).
5. (Optionnel) Assignez le `WaterTileMap` de votre scène pour activer la validation.

## Déclenchement
Le peuplement initial est déclenché dans le `_Ready()`. Le système vérifie ensuite périodiquement (`RespawnInterval`) s'il doit faire réapparaître des ressources manquantes.

## Tags
`Gameplay`, `Map`, `Procedural`, `Spawning`, `Algorithme`
