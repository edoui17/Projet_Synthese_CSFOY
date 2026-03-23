# Convention de Travail - Standards de Développement

Ce document définit les normes de codage, les flux de travail et les principes éthiques à respecter pour assurer la cohérence, la qualité et la maintenabilité de nos projets.

---

## 1. Langue et Terminologie
* **Code :** Tout le code (nommage des variables, classes, méthodes, etc.) doit être écrit en **anglais**.
* **Commentaires :** Utiliser des commentaires pour expliquer les parties complexes du code. Le code doit être le plus explicite possible par lui-même.
* **Sobriété :** Ne jamais utiliser d'**émojis** dans le code source ou dans les commentaires.

## 2. Règles de Nommage (Casing)

| Élément | Format | Exemple |
| :--- | :--- | :--- |
| **Dossiers** | PascalCase | `UserServices/`, `DataModels/` |
| **Classes, Interfaces, Enums** | PascalCase | `UserRepository`, `ILogger` |
| **Propriétés et Méthodes** | PascalCase | `CalculateTotal()`, `IsActive` |
| **Constantes** | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT`, `API_URL` |
| **Variables locales** | camelCase | `currentUser`, `totalAmount` |
| **Données membres (Champs)** | m_camelCase | `m_databaseContext` |
| **Paramètres de méthodes** | p_camelCase | `p_userId`, `p_requestPayload` |

## 3. Architecture et Structure
* **Abstractions :** Toujours créer une **interface** avant d'implémenter la logique de code.
* **Organisation :** Regrouper les classes et les fonctionnalités similaires dans des dossiers dédiés.
* **Clarté :** Utiliser des noms de variables et de méthodes **explicites**. Éviter les abréviations non évidentes.
* **Logique de boucle :** Privilégier l'utilisation de `forEach` au lieu de `for` lorsque c'est possible pour améliorer la lisibilité.
* **Gestion des erreurs :** Prioriser l'utilisation de blocs `try-catch` pour sécuriser l'exécution.

## 4. Documentation et Référencement
* **Importance de la Documentation :** Chaque module ou fonctionnalité majeure doit être documenté (README ou documentation technique) pour faciliter la passation et la maintenance.
* **Référencement des sources :** Si une solution technique provient d'une source externe (StackOverflow, documentation officielle, tutoriel), le lien doit être ajouté en commentaire pour permettre aux futurs développeurs de comprendre l'origine du code.
* **Plagiat :** Le plagiat de code propriétaire est strictement interdit. Le code utilisé doit respecter les licences logicielles en vigueur.

## 5. Utilisation de l'IA
* **Support, non substitut :** L'Intelligence Artificielle doit être utilisée comme un outil d'aide à la conception, au débogage ou à l'optimisation.
* **Validation humaine :** Tout code généré par IA doit être **revu, compris et testé** par le développeur avant d'être intégré au projet.

## 6. Gestion des Branches et Git
* **Branches :** Créer une branche avec un **nom explicite** pour chaque *User Story*.
* **Focus :** Se concentrer sur une seule tâche à la fois et la terminer complètement avant d'en commencer une nouvelle.
* **Protection Master :** Il est strictement interdit de faire un commit sur `Master` sans que le code n'ait été testé au préalable.

## 7. Tests et Déploiement
* **Validation continue :** Tester systématiquement le code après chaque modification.
* **Nettoyage :** Lors du déploiement, s'assurer qu'**aucun dossier de test** n'est inclus dans l'environnement de production.