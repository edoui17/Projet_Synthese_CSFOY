# IslandSurvivor (Jeu Godot)

Le client de jeu développé avec le moteur **Godot 4.6.1 (.NET)**.

### Organisation interne :
* `/Assets` : Ressources visuelles et sonores.
* `/Scenes` : Arborescence des niveaux et de l'UI.
* `/Classes` : Scripts C# gérant la logique du gameplay.
* `/Globals` : Scripts utilisés comme **Autoloads** (Singletons). 

### Intégration :
Référence le projet `Core` pour utiliser les mêmes modèles de données lors de l'envoi des statistiques à l'API.