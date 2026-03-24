# **Définition** de la cible du projet pour la session

## **Participants**
1. Charles-Philippe Warren
2. Delphine Martin
3. Edouard Couture
4. Kévin Houle
5. Antoine Masson

## **Énumération des besoins exprimés par le client, soit en rencontre formelle ou en fonction de la documentation disponible.**
- Réalisation d’une liste de vérification de l’état du projet en fonction des besoins connus (À Faire, En Cours et Fait)
- Priorisation des besoins à combler pour la session
## **Technologies utilisées.**

- #En production 
- [ ] Diagramme de l’architecture de production (serveurs, services et liens entre eux)
- [X] Services utilisés

* Base de données : Microsoft SQL Server (MSSQL). Stockage persistant des joueurs, scores et statistiques.

* Backend / API : ASP.NET Core. Sert de pont entre le jeu (Godot) et la base de données pour sécuriser les transactions.

* Frontend Web : Blazor (ASP.NET). Interface utilisateur pour la consultation des classements et du profil.

* **Client de jeu :** Godot Engine (C#). Application lourde exécutée par l'utilisateur final.
- [X] Méthodologie d’utilisation des services
* Le jeu communique avec l'API via des requêtes sécurisées pour mettre à jour le `HighScore` et les ressources. L'accès direct à la base de données est réservé à l'API pour garantir l'intégrité des données (prévention de la triche).


- #En développement
- [ ] Diagramme de l’architecture de production (serveurs, services et liens entre eux)
- [X] Services utilisés (peut y en avoir plusieurs et doivent être documentés)
* IDE : Visual Studio (C#) pour le développement backend et web.

* Moteur de jeu : Godot (version .NET/C#).

* Docker Desktop : Utilisation d'une image officielle `mcr.microsoft.com/mssql/server` pour faire tourner la base de données localement sans installation lourde.

* ORM : Entity Framework Core pour la gestion des migrations de schémas (Code-First).
- [X] Méthodologie d’utilisation des services
* Chaque développeur lance un conteneur Docker local pour la base de données. Cela permet d'isoler les tests et de s'assurer que l'environnement est identique pour tous les membres de l'équipe (évite le "ça marche sur ma machine").


- #En déploiement
- [X] Services utilisés (peut y en avoir plusieurs et doivent être documentés)
* Hébergement : À valider (Probablement Azure ou un serveur interne du Cégep).

* Registre d'images : Docker Hub (pour stocker et versionner les images du serveur web et de la BD).

- [ ] Localisation de l’hébergement des services
- [X] Méthodologie d’utilisation des services
* Le déploiement automatisé (CI/CD) est privilégié. Lors d'un merge sur `Master`, une image Docker est construite et poussée sur le registre, puis déployée sur le serveur de production. 

*Note : Aucun dossier de test ou données de test n'est envoyé lors de la création de l'image de production.*

- [ ] Estimation des coûts
## **État de situation**
- Statut du projet 
- [ ] État actuel
    1. Ce qui est fonctionnel en production: Rien n'est fonctionnel en production nous commençons un nouveau projet de zéros.
    2. Ce qui est en développement: Rien n'est fonctionnel en développement nous commençons un nouveau projet de zéros.
    3. Ce qui est en déploiement ou près du déploiement: Rien n'est fonctionnel en déploiement nous commençons un nouveau projet de zéros.