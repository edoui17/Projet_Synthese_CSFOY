# API (Backend)

Ce projet est une **Web API ASP.NET Core**. Elle sert de pont entre le jeu (Godot) et la base de données.

### Responsabilités :
* Recevoir les statistiques envoyées par le client de jeu.
* Authentifier les requêtes (via Auth0 ou JWT).
* Fournir les données nécessaires au tableau de bord Web (Blazor).

### Dépendances :
* `Core` : Pour les modèles de données partagés.
* `Infrastructure` : Pour l'accès à la base de données SQL.