# Projet Core (Logique Partagée & Abstractions)

## 📌 Responsabilité Unique
Ce répertoire est le cœur battant de notre architecture N-Tiers. Il contient les règles métier pures, les entités du domaine, la logique mathématique (calcul d'XP, formules de combat) et les abstractions (Interfaces).
**Il est agnostique et représente la source de vérité absolue ("Single Source of Truth").**

## ✅ Composants Autorisés
- Interfaces des services (`Interfaces/`)
- Modèles de données purs (`Domain/` ou `Models/`)
- Utilitaires mathématiques et algorithmiques (`Logic/`, `Utils/`)
- POCOs pour les événements (`Events/`)

## 🚫 Dépendances Interdites
- **Aucune dépendance externe majeure.**
- **Godot** : Interdiction totale de référencer des namespaces liés au moteur (`Godot.*`).
- **Entity Framework** : Interdiction d'utiliser `Microsoft.EntityFrameworkCore` ou des attributs spécifiques aux bases de données (ex: `[Table]`). L'infrastructure s'occupera du mapping.

## 💡 Conseils pour l'équipe (et l'IA)
- Pensez "Interfaces First". Toute fonctionnalité majeure doit d'abord être définie par un contrat ici.
- Le code ici doit être 100% testable unitairement, sans avoir besoin de mocker une base de données complexe ou de démarrer un moteur de jeu.

- **Logique Applicative de Gameplay** : Interdiction absolue d'inclure de la logique spatio-temporelle, des calculs de vecteurs 2D, des timers actifs, de la physique ou de la gestion d'états d'animation. Le `Core` ne simule pas le jeu, il compte et structure les données.
