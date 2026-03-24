# Core (Logique Partagée)

C'est le projet le plus important de la solution. Il contient tout ce qui est **commun** à l'ensemble du système.

### Contenu :
* **Models/** : Classes de données (ex: `PlayerStats`, `MatchResult`).
* **Interfaces/** : Contrats pour les services (ex: `IStatsService`).
* **Logic/** : Algorithmes de calcul pur (XP, Elo, etc.) testables unitairement.

> **Note :** Ce projet ne doit avoir aucune dépendance vers les autres projets de la solution (Single Source of Truth).