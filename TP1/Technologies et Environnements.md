# 🛠 Technologies et Environnements

Ce document détaille l'infrastructure technique, les choix technologiques et les processus de gestion des sources du projet.

---

## 1. Architecture de Production (Serveurs & Services)

### Diagramme de l'architecture de production
> *Note : Un diagramme visuel montrant le client Godot communiquant avec l'API ASP.NET Core, qui elle-même interroge la base de données MSSQL, devrait être inséré ici.*


![Diagramme de l'architecture de production](../Images/DiagrammeArchitectureProduction.jpg)

### Services utilisés
* **Base de données :** Microsoft SQL Server (MSSQL). Stockage persistant des joueurs, scores et statistiques.
* **Backend / API :** ASP.NET Core. Sert de pont entre le jeu (Godot) et la base de données pour sécuriser les transactions.
* **Frontend Web :** Blazor (ASP.NET). Interface utilisateur pour la consultation des classements et du profil.
* **Client de jeu :** Godot Engine (C#). Application lourde exécutée par l'utilisateur final.

### Méthodologie
Le jeu communique avec l'API via des requêtes sécurisées pour mettre à jour le `HighScore` et les ressources. L'accès direct à la base de données est réservé à l'API pour garantir l'intégrité des données (prévention de la triche).

---

## 2. Architecture de Développement

### Diagramme de l'architecture de développement
> *Note : Représente les postes locaux des développeurs utilisant Visual Studio et Docker Desktop.*

![Diagramme de l'architecture de production](../Images/DiagrammeArchitectureDeveloppement.jpg)

### Services utilisés
* **IDE :** Visual Studio (C#) pour le développement backend et web.
* **Moteur de jeu :** Godot (version .NET/C#).
* **Docker Desktop :** Utilisation d'une image officielle `mcr.microsoft.com/mssql/server` pour faire tourner la base de données localement sans installation lourde.
* **ORM :** Entity Framework Core pour la gestion des migrations de schémas (Code-First).

### Méthodologie
Chaque développeur lance un conteneur Docker local pour la base de données. Cela permet d'isoler les tests et de s'assurer que l'environnement est identique pour tous les membres de l'équipe (évite le "ça marche sur ma machine").

---

## 3. Déploiement

### Services utilisés et localisation
* **Hébergement :** À valider (Probablement Azure ou un serveur interne du Cégep).
* **Registre d'images :** Docker Hub (pour stocker et versionner les images du serveur web et de la BD).

### Méthodologie de déploiement
Le déploiement automatisé (CI/CD) est privilégié. Lors d'un merge sur `Master`, une image Docker est construite et poussée sur le registre, puis déployée sur le serveur de production. 
*Note : Aucun dossier de test ou données de test n'est envoyé lors de la création de l'image de production.*

### Estimation des coûts
| Service | Coût estimé | Note |
| :--- | :--- | :--- |
| **Hébergement Web/API** | 0$ - 15$/mois | Selon le palier Azure Student ou local. |
| **Base de données MSSQL** | 0$ - 30$/mois | Souvent inclus dans les forfaits cloud. |
| **Docker Hub** | Gratuit | Plan personnel suffisant. |

---

## 4. Gestion des Sources (Git)

### Branches principales
* **`master` (ou `main`) :** Branche de production. Code stable et testé.
* **`dev` :** Branche d'intégration. Regroupe les fonctionnalités terminées avant le passage en production.

### Procédures de fusion (Merge Requests)
L'équipe applique des règles de révision strictes pour garantir la qualité du code :

1.  **Vers la branche `dev` :**
    * Nécessite **une (1) revue** par un membre n'ayant pas contribué à la tâche.
    * Validation obligatoire par **Jules**.
2.  **Vers la branche `master` :**
    * Nécessite **deux (2) revues** par des membres tiers (excluant l'auteur).
    * Validation obligatoire par **Jules**.
    * Tests de non-régression obligatoires.

---

## 5. Schéma de Données (Aperçu)
* **Table User :** Comptes, authentification.
* **Table Player :** IDUnique, Nom, Experience, Ressources, KillCount.
* **Table HighScore :** Lien via IDUnique, Score, Date de performance.