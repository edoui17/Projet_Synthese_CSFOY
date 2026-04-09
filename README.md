# IslandSurvivor

Bienvenue dans le projet **IslandSurvivor**. Il s'agit d'un projet de jeu vidéo de survie intégrant une architecture moderne en couches (N-Tier) pour la collecte et l'affichage de statistiques en temps intéractif.

---

## Architecture de la Solution

Le projet est structuré pour maximiser le partage de code entre le client de jeu et les services web, garantissant une cohérence des données.



### Organisation des dossiers
* **`/Src`** : Contient tous les projets de production.
    * `IslandSurvivor` : Le client de jeu (Godot 4.x .NET).
    * `API` : Backend ASP.NET Core pour la gestion des données.
    * `Core` : Bibliothèque de classes partagée (Modèles et Logique).
    * `Infrastructure` : Persistance SQL avec Entity Framework Core.
    * `Web` : Dashboard de statistiques (Blazor).
* **`/Tests`** : Projets de tests unitaires et d'intégration (xUnit).
* **`/Convention`** : Normes de codage et guides de style du projet.
* **`/WikiCode`** : Documentation technique détaillée.

---

## Technologies utilisées

* **Moteur de jeu :** Godot 4.6.1 (C# / .NET 8)
* **Backend :** ASP.NET Core Web API
* **Frontend Web :** Blazor Web App
* **Base de données :** SQL Server (EF Core)
* **Tests :** xUnit
* **IDE :** Visual Studio 2022

---

##  Nomenclature complète des Tags

###  Gameplay & Monde
* **`Gameplay`** : Utilisé pour toutes les mécaniques de jeu actives (mouvement, récolte, combat).
* **`Mouvement`** : Spécifique à la locomotion du personnage et aux contrôles de déplacement.
* **`Interaction`** : Concerne le système de détection (Triggers) et l'activation d'objets dans le monde.
* **`Map`** : Tout ce qui touche à l'environnement, aux niveaux et aux îles.
* **`Procedural`** : Identifie les algorithmes de génération aléatoire de terrain.
* **`Spawning`** : Logique d'apparition dynamique des ressources et des entités sur la carte.
* **`Navigation`** : Gestion des transitions entre les scènes et du voyage entre les îles.

###  Systèmes & Données
* **`Statistique`** : Calculs des points de vie (PV), de la force et des modificateurs de progression.
* **`Ressource`** : Logique de collecte (Ajout) et de dépense (Améliorations/Déblocages).
* **`Score`** : Système de points, calcul du High Score et préparation pour les classements.
* **`Système`** : Architecture logicielle globale, gestionnaires (Managers) et persistance des données.
* **`Algorithme`** : Tâches nécessitant une logique mathématique complexe (Génération, Distribution).

###  Interface & Contrôles
* **`UI`** : (User Interface) Tous les éléments graphiques, boutons et fenêtres.
* **`ATH`** : (Affichage Tête Haute) Éléments de l'interface visibles en jeu (Barres de vie, compteurs).
* **`Menu`** : Navigation dans les écrans hors-jeu (Principal, Pause, Gestion).
* **`Input`** : Gestion des entrées utilisateur (Clavier, Souris, Manette).
* **`Physique`** : Tout ce qui implique les collisions (Collider2D) et les interactions avec le moteur physique.

---

## Stratégie de Qualité (CI/CD Ready)

Pour assurer la stabilité du projet, nous appliquons une approche de **tests automatisés** :
1. **Tests Unitaires** : Validation de la logique mathématique et des règles métier dans le projet `Core`.
2. **Tests d'Intégration** : Vérification des flux de données entre l'API et la base de données SQL.
3. **Architecture Partagée** : L'utilisation du projet `Core` empêche toute divergence de modèle entre le Jeu et le Web.

---

## Installation et Lancement

### Prérequis
* Visual Studio 2022 (avec la charge de travail .NET)
* Godot 4.x (Version .NET)
* SDK .NET 8.0+

### Étapes
1. Cloner le dépôt.
2. Ouvrir `ProjetJeu.sln` dans Visual Studio 2022.
3. Restaurer les packages NuGet.
4. Appliquer les migrations SQL :
   `Update-Database -Project Infrastructure`
5. Lancer la solution (Projets de démarrage multiples : API + Web).
6. Ouvrir Godot pour lancer le client `IslandSurvivor`.

---

## Auteur
**Kevin Houle** - Étudiant en programmation au Cégep de Sainte-Foy.<br>
**Edouard Couture** - Étudiant en programmation au Cégep de Sainte-Foy.<br>
**Antoine Masson** - Étudiant en programmation au Cégep de Sainte-Foy.<br>
**Delphine Martin** - Étudiante en programmation au Cégep de Sainte-Foy.<br>
**Charles-Phillipe Warren** - Étudiant en programmation au Cégep de Sainte-Foy.
---

## Technical Information: NPC Entities

In IslandSurvivor, NPCs are separated into two distinct types logically:
* **Passive** (e.g., Sheep)
* **Hostile** (e.g., Enemies)

### Sheep (Passive) & Resource Looting
The Sheep is a passive entity that roams the island idly. When it takes damage from the player or any node, its `SheepController.cs` logic triggers a "Flee" state, which makes it move faster in the opposite direction of the attacker.

Upon death, the loot (Meat) is distributed **directly to the global inventory system**. The Sheep uses the `SignalManager.Instance.EmitMaterialDestroyed(...)` method to send the `ResourceItem` data (amount and type) up to the Godot Event Bus. The `InventoryNode` automatically detects this signal and processes the loot, meaning there is no loose drop left on the floor.

(Addendum): The Sheep loot generation verifies if the attacker was the Player (via group check "Player") before instantiating and adding exactly 1 meat item, thus preventing the economy from breaking due to environmental deaths or non-player damage. Additionally, NavigationAgent2D has been wired up to compute valid velocities ensuring safe obstacle avoidance.
