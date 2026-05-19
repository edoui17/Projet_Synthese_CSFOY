Documentation : Système de Spawn d'Ennemis (EnemySpawnZone)
Vue d'ensemble
Le système EnemySpawnZone est responsable de la génération et de la gestion dynamique des ennemis dans une zone spécifique. Il utilise un système de poids (weights) pour gérer les probabilités d'apparition de différents types d'ennemis, tout en respectant une limite de population et des contraintes environnementales (eau, distance entre entités).

Architecture
Le système suit une architecture découplée avec une approche "Interface First".

1. Core (Logique Pure)
IEnemySpawnZone : Interface définissant le contrat pour le déclenchement du spawn.
IWeightedItem : Interface utilisée pour la sélection aléatoire pondérée des types d'ennemis.
2. IslandSurvivor (Godot)
EnemySpawnZone (Node2D) : Contrôleur principal de la zone.
Utilise un enfant Polygon2D nommé "SpawningArea" pour délimiter la zone de spawn.
Gère un timer interne pour le Respawn automatique.
Nettoie automatiquement les références aux ennemis détruits.
EnemySpawnConfig (Resource) : Ressource personnalisée ([GlobalClass]) liant une scène d'ennemi (PackedScene) à un poids de probabilité (Weight).
Paramètres Configurables
Paramètre	Description	Défaut
EnemyConfigs	Liste des configurations d'ennemis (Scène + Poids).	[]
MaxEnemies	Nombre maximum d'ennemis autorisés dans cette zone.	5
SpawningArea	Référence au polygone définissant l'aire de spawn.	null
WaterTileMap	Couche de tuiles d'eau pour éviter le spawn sur l'eau.	null
MinDistanceBetweenEnemies	Distance minimale entre deux ennemis pour éviter les chevauchements.	100f
RespawnInterval	Délai en secondes entre chaque vérification de respawn.	60f
Fonctionnement
Initialisation : Au démarrage (_Ready), le système détecte le polygone, calcule ses limites et remplit la zone jusqu'à MaxEnemies.
Validation : Pour chaque ennemi, le système choisit un point aléatoire dans les limites du polygone et vérifie :
Si le point est réellement dans la forme du polygone (IsPointInPolygon).
Si le point ne correspond pas à une tuile d'eau dans le WaterTileMap.
Si aucun autre ennemi n'est déjà présent dans le rayon MinDistanceBetweenEnemies.
Mise à jour : Le système vérifie périodiquement si des ennemis ont été vaincus et en génère de nouveaux pour maintenir la population à MaxEnemies.
Utilisation
Créer un node Node2D et lui attacher le script EnemySpawnZone.cs.
Ajouter un enfant Polygon2D nommé "SpawningArea".
Dessiner la zone souhaitée avec l'outil polygone de Godot.
Dans l'inspecteur, créer/ajouter des EnemySpawnConfig dans la liste EnemyConfigs.
Glisser-déposer les scènes d'ennemis (héritant de EnemyBase) dans les configs et ajuster les Weights.
Assignez la WaterTileMap de votre niveau pour garantir un spawn sur la terre ferme.
Tags
Gameplay, Spawning, Algorithme, Map, NPC