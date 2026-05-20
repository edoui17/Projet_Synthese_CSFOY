# Convention de Travail - Standards de Développement

Ce document définit les normes de codage, les flux de travail et les principes éthiques à respecter pour assurer la cohérence, la qualité et la maintenabilité de nos projets.

---

## 1. Langue et Terminologie
* **Code :** Tout le code (nommage des variables, classes, méthodes, etc.) doit être écrit en **anglais** pour garantir une compatibilité universelle avec les bibliothèques et moteurs (ex: Godot).
* **Commentaires :** Utiliser des commentaires pour expliquer le *pourquoi* (l'intention) des parties complexes du code, et non le *comment*. Le code doit être le plus explicite possible par lui-même.
* **Sobriété :** Ne jamais utiliser d'**émojis** dans le code source ou dans les commentaires.

## 2. Règles de Nommage (Casing)

| Élément | Format | Exemple |
| :--- | :--- | :--- |
| **Dossiers & Espaces de noms (Namespaces)**| PascalCase | `UserServices/`, `DataModels/` |
| **Classes, Structs, Records, Enums** | PascalCase | `UserRepository`, `GameState` |
| **Interfaces** | IPascalCase | `ILogger`, `IStatManager` |
| **Propriétés et Méthodes** | PascalCase | `CalculateTotal()`, `IsActive` |
| **Nœuds Godot (Nodes) & Scènes** | PascalCase | `PlayerController`, `SpawnManager` |
| **Signaux (Godot)** | PascalCase + EventHandler | `HealthChangedEventHandler` |
| **Constantes** | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT`, `API_URL` |
| **Variables locales** | camelCase | `currentUser`, `totalAmount` |
| **Données membres (Champs privés)** | m_camelCase | `m_databaseContext`, `m_healthPoints` |
| **Attributs Exportés (Godot `[Export]`)** | PascalCase | `[Export] public int MoveSpeed { get; set; }`|
| **Paramètres de méthodes** | p_camelCase | `p_userId`, `p_requestPayload` |

## 3. Architecture et Structure (C# & Godot)
* **Séparation "Accountant vs. Orchestrator" :** Maintenir une séparation stricte entre la logique d'affaires (l'Accountant : les mathématiques, les données, gérés par exemple dans un dossier `Src/Core`) et l'exécution visuelle (l'Orchestrator : le client Godot, les animations, les signaux).
* **Abstractions :** Toujours créer une **interface** avant d'implémenter la logique de code majeure pour faciliter les tests et l'injection de dépendances.
* **Précision du vocabulaire :** Utiliser des noms de variables et de méthodes **spécifiques et explicites**. (Ex: Privilégier `BaseAttackValue` et `InitialAttackPoints` au lieu d'un simple `BaseDamage` ambigu).
* **Early Returns (Guard Clauses) :** Éviter l'imbrication profonde de conditions `if`. Vérifier les conditions d'erreur ou les cas de base en début de méthode et retourner immédiatement.
* **Logique de boucle :** Privilégier l'utilisation de `foreach` ou des requêtes LINQ simples pour la lisibilité, *sauf* dans les boucles critiques (`_Process`, `_PhysicsProcess`) où la performance dicte l'utilisation de boucles `for` pour éviter la pression sur le Garbage Collector.
* **Gestion des erreurs :** Prioriser l'utilisation de blocs `try-catch` pour sécuriser l'exécution, et valider l'état des instances avec `IsInstanceValid()` dans Godot avant d'agir sur un nœud.

## 4. Documentation et Référencement
* **Importance de la Documentation :** Chaque module ou fonctionnalité majeure doit être documenté (README ou documentation technique) pour faciliter la passation et la maintenance.
* **Référencement des sources :** Si une solution technique provient d'une source externe (StackOverflow, documentation officielle, tutoriel), le lien doit être ajouté en commentaire.
* **Plagiat :** Le plagiat de code propriétaire est strictement interdit. Le code utilisé doit respecter les licences logicielles en vigueur.

## 5. Utilisation de l'IA
* **Support, non substitut :** L'Intelligence Artificielle doit être utilisée comme un outil d'aide à la conception, au débogage ou à l'optimisation.
* **Validation humaine :** Tout code généré par IA doit être **revu, compris et testé** par le développeur avant d'être intégré au projet.

## 6. Gestion des Branches et Git
* **Branches :** Créer une branche en incluant **le numéro de la User Story au début** suivi d'un nom explicite (ex : `feature/US12-inventory-persistence`, `refactor/US34-stat-manager`).
* **Focus :** Se concentrer sur une seule tâche à la fois et la terminer complètement avant d'en commencer une nouvelle.
* **Protection Master :** Il est strictement interdit de faire un commit direct sur `Master` sans que le code n'ait été testé et revu au préalable.

## 7. Tests et Déploiement
* **Validation continue :** Tester systématiquement le code après chaque modification.
* **Nettoyage :** Lors du déploiement, s'assurer qu'**aucun dossier de test** n'est inclus dans l'environnement de production.