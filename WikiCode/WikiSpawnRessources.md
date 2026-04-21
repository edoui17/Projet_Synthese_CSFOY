# Documentation : Système de Spawn Aléatoire de Ressources (Arbres)

## Vue d'ensemble
Le système de spawn d'arbres permet de peupler dynamiquement la carte avec différents modèles d'arbres lors du chargement du niveau, assurant une diversité visuelle sans intervention manuelle.

## Architecture
Le système suit une architecture N-Tier avec une approche "Interface First".

### 1. Core (Logique Pure)
- **`IRandomSelector<T>`** : Interface générique définissant la capacité à choisir un élément aléatoire.
- **`RandomSelector<T>`** : Implémentation basée sur `System.Random`.

### 2. IslandSurvivor (Godot)
- **`TreeSpawn` (Node : Marker2D)** : Point de spawn placé dans les scènes.
    - Scanne dynamiquement le dossier `res://Scenes/Ressources/Tree`.
    - Sélectionne un fichier `.tscn` aléatoirement via le `IRandomSelector`.
    - Instancie l'arbre en tant qu'enfant.
- **`TreePopulationManager` (Node)** : Coordinateur central.
    - Parcourt l'arbre de scènes pour trouver tous les `TreeSpawn`.
    - Déclenche la méthode `SpawnTree` sur chaque point trouvé.

## Utilisation

### Ajouter de nouveaux modèles d'arbres
Il suffit de placer les nouvelles scènes d'arbres (`.tscn`) dans le dossier :
`Src/IslandSurvivor/Scenes/Ressources/Tree`

Le système les détectera automatiquement au prochain lancement.

### Placer un point de spawn
1. Instancier la scène `SpawnPointTree.tscn` dans votre niveau.
2. Positionner le node là où vous souhaitez qu'un arbre apparaisse.

## Déclenchement
Le peuplement est actuellement déclenché dans `Spawn.cs` juste avant l'instanciation du joueur via `TreePopulationManager.PopulateTrees()`.

## Tags
`Gameplay`, `Map`, `Procedural`, `Spawning`, `Algorithme`
