#  IslandSurvivor - Technical Log

##  Architecture & Interopérabilité (N-Tier)
* **Séparation Core/Client** : Le `Core` est indépendant de Godot. Les domaines purs ne peuvent pas parser d'assets (ex: `Texture2D`). Les items utilisent des chemins absolus (`string IconPath = "res://Assets/..."`) que le client Godot convertit en images.
* **Gestion des Événements (Bridge Pattern)** : Implémentation via les `WeakEvents` du Core mappés sur les Singletons de Godot. Le `SignalManager` de Godot sert de proxy ("glue") pour la logique métier.
* **Persistance des Singletons** : Pour maintenir l'état lors du rechargement de scènes, la classe `InventoryNode` utilise les mécanismes d'Autoload de Godot et des instances statiques pour son implémentation `.NET` (`InventoryManager`).

##  Génération Procédurale de Map
* **Approche Hybride** : `IMapGenerator` est défini dans le Core, mais l'implémentation concrète `GodotIslandGenerator` réside dans le projet Godot pour exploiter `FastNoiseLite`.
* **Pont de Données** : Le Core utilise des constantes de type string (`"Water"`, `"Ground"`) que la scène Godot traduit en coordonnées Atlas `Vector2I` pour le `TileMapLayer`.
* **Contraintes d'Élévation** : 
    * Ajout de Plateaux, Falaises et Escaliers.
    * Utilisation d'un algorithme **BFS** pour détecter les bordures de plateaux (Falaises) et garantir l'accès via des tuiles Escaliers.
    * Vérification de la connectivité globale pour assurer que l'île reste traversable.
* **Rendu et Visualisation** :
    * `MapRenderer` lit les données via le `GameManager` (Singleton).
    * Utilisation de `SetCellsTerrainConnect()` en batch pour optimiser le rendu.
    * **Animations de mousse (Foam/Splash)** : Logique de détection strictement côté client (adjacence Eau/Terre). Utilisation d'un `FoamWaterTileMap` superposé.
    * **Tri de profondeur** : `FoamWaterTileMap` placé sous le `GroundTileMap` dans `map_1.tscn` pour que les effets dépassent de dessous les tuiles de sol.

##  Implémentation du Mouton (US 4.4 - Passive NPC)
* **Logique Découplée** : Les calculs de fuite (Flee) et les timers tournent en C# pur (`SheepController`). Le nœud Godot transmet simplement le `delta`.
* **Configuration Sheep.tscn** :
    1.  **Root** : `CharacterBody2D` nommé "Sheep".
    2.  **Enfants** : `Sprite2D`, `CollisionShape2D`, et `NavigationAgent2D` (préparation pour le futur pathfinding).
    3.  **Script** : Attacher `Sheep.cs`. Configurer `IdleSpeed` (30.0), `FleeSpeed` (120.0) et `MaxHealth` (3) dans l'inspecteur.
    4.  **Collisions** : Layer "Enemy/NPC", Mask "World".
* **Système de Loot** : Le script émet automatiquement `SignalManager.Instance.EmitMaterialDestroyed(...)` à la mort pour que l'inventaire reçoive la ressource "Viande".

##  Systèmes UI et Inventaire
* **UI Dynamique** : Création de popups interactifs dans `InteractionScript.cs` (`CanvasLayer`, `Panel`, `Button`). Réagit aux entrées du joueur dans l' `Area2D` et gère les clics via des actions lambda.
* **Isolation des Stats** : Les améliorations de statistiques émettent `StatUpgradePurchased` au lieu de coupler le script d'interaction au `StatManager`.
* **Quirks Techniques** : 
    * Évitement des Enums au profit de constantes string statiques (`SheepStates`).
    * Approche "Interface-First" (`INpc`, `IDamageable`, `IHealthComponent`).