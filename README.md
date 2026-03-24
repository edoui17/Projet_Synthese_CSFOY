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