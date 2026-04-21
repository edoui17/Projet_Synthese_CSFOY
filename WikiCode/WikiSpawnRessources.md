# Documentation : Système de Spawn Aléatoire de Ressources (TreeZone)

## Vue d'ensemble
Le système `TreeZone` permet de peupler dynamiquement des zones spécifiques de la carte avec un nombre défini d'arbres. Contrairement à l'ancien système de points individuels, `TreeZone` utilise une zone polygonale pour définir l'aire de répartition.

## Architecture
Le système suit une architecture N-Tier avec une approche "Interface First".

### 1. Core (Logique Pure)
- **`ITreePopulator`** : Interface définissant la capacité à peupler une zone avec des arbres.

### 2. IslandSurvivor (Godot)
- **`TreeZone` (Node : Node2D)** : Définit une zone de peuplement.
    - Utilise un enfant **`Polygon2D`** (nommé "SpawningArea") pour définir la forme de la zone.
    - Scanne dynamiquement le dossier `res://Scenes/Ressources/Tree`.
    - Génère des positions aléatoires dans le polygone via `Geometry2D.IsPointInPolygon`.
    - Instancie les arbres en tant qu'enfants du `TreeZone`.
    - Force la visibilité, l'échelle et la couche de collision (Layer 5 : Ressource).

## Utilisation

### Ajouter de nouveaux modèles d'arbres
Il suffit de placer les nouvelles scènes d'arbres (`.tscn`) dans le dossier :
`Src/IslandSurvivor/Scenes/Ressources/Tree`

Le système les détectera automatiquement au prochain lancement.

### Créer une zone de peuplement
1. Créez un node `Node2D` et attachez-lui le script `TreeZone.cs`.
2. Ajoutez un enfant `Polygon2D` nommé **"SpawningArea"**.
3. Dessinez la forme de la zone dans l'éditeur Godot en utilisant les points du polygone.
4. Ajustez la propriété **`TreeCount`** dans l'inspecteur pour définir le nombre d'arbres souhaités.

## Déclenchement
Le peuplement est déclenché automatiquement dans le `_Ready()` du node `TreeZone`.

## Tags
`Gameplay`, `Map`, `Procedural`, `Spawning`, `Algorithme`
