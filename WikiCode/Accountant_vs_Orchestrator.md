# L'Architecture "Accountant vs. Orchestrator"

Ce document définit l'un des piliers architecturaux fondamentaux d'**IslandSurvivor** : la séparation stricte entre le Core (le "Comptable") et le Client Godot (l'"Orchestrateur"). Cette architecture N-Tier garantit que la logique métier du jeu reste totalement découplée du moteur de jeu et de la base de données.

## 1. Le Core : Le "Comptable" (Accountant)

Le projet `Src/Core` est responsable de la logique d'affaires (business logic), des mathématiques, des statistiques et des règles du jeu. Il se comporte comme un comptable agnostique : il ne voit que des chiffres et des règles de calcul.

### Règles strictes du Core :
*   **Zéro Dépendance Externe :** Le Core ne doit **jamais** référencer `Godot.*`, `Microsoft.EntityFrameworkCore` ou toute autre bibliothèque spécifique à la vue ou à la persistance.
*   **Zéro Logique Spatio-temporelle :** Aucun concept de temps (`_Process`, `DeltaTime`), d'espace ou de physique (`Vector2`, collisions) n'a sa place dans le Core. Même si l'on est tenté d'utiliser `System.Numerics.Vector2` pour contourner la restriction Godot, les algorithmes de déplacement, les calculs de trajectoire et l'interpolation temporelle doivent rester dans le client Godot (`IslandSurvivor/Logic/`).
*   **Collections Natives .NET :** Le Core utilise exclusivement des collections C# natives (`System.Collections.Generic.List<T>`, `Dictionary`, `HashSet`) pour maximiser les performances et permettre l'utilisation de LINQ.
*   **Abstractions Déterministes :** Toute interaction avec des fonctions aléatoires ou de journalisation doit passer par des interfaces explicites définies dans `Core.Interfaces.Utils` (ex. `ILogger`, `IRandomProvider`). L'implémentation Godot (ex. via `GD.Randf()`) sera injectée par le client.
*   **Communication Isolée :** Le Core ne communique vers l'extérieur ou avec ses autres modules qu'en publiant ou en souscrivant à des événements fortement typés (`IEvent`) via le système **EventBus**.

## 2. Le Client Godot : L'"Orchestrateur" (Orchestrator)

Le projet `Src/IslandSurvivor` représente la vue et le contrôleur dans le moteur de jeu Godot. Il "orchestre" le rendu graphique, le traitement des entrées du joueur (inputs) et la physique.

### Règles strictes du Client Godot :
*   **Logique de Représentation Exclusive :** Les scripts attachés aux nœuds Godot gèrent les états visuels (`AnimatedSprite2D`, `_PhysicsProcess`, hitboxes). Par exemple, la logique d'un Dash ou les limites d'un "cooldown" temporel vivent ici.
*   **Aucun Accès Direct à l'Infrastructure :** L'Orchestrateur (Godot) n'a aucune connaissance de la base de données ou de l'Entity Framework. Pour persister des données, il s'adresse à l'API via des requêtes (ex. `/api/player/sync`).
*   **Pattern Bridge (Pont) :** Le client Godot sert de pont. Lorsqu'une action physique se produit (ex. ramasser une ressource), le client Godot collecte les informations et envoie un événement C# standard sur l'**EventBus**. De même, des nœuds globaux (comme `SignalManager` ou `StatManager`) écoutent l'EventBus du Core pour émettre des `[Signal]` natifs Godot et mettre à jour l'Interface Utilisateur (UI).
*   **Collections Godot Limité :** Les collections de Godot (`Godot.Collections.Array`, `Dictionary`) sont strictement réservées aux variables exposées dans l'Inspecteur via `[Export]` ou lorsqu'une API native Godot les exige. Ailleurs, les collections natives .NET sont de mise pour éviter le surcoût de conversion entre C# et C++ (Marshalling overhead).

## 3. Pourquoi cette séparation ? (Le "Pourquoi")

Si ce modèle peut sembler imposer des contraintes importantes de prime abord, il apporte d'immenses avantages pour la durée de vie du projet :
1.  **Testabilité Unitaire (100% Coverage) :** Parce que le Core ne dépend pas du système de scène Godot, les règles du jeu (calculs d'XP, calcul des dommages, probabilités de loot) peuvent être testées unitairement de manière exhaustive avec `xUnit` de façon instantanée.
2.  **Portabilité et Agnosticisme :** Demain, si le projet change de moteur de jeu ou de plateforme serveur, le projet `Core` entier sera réutilisable sans modifier la moindre ligne de code de logique pure.
3.  **Résilience de la Mémoire :** La stricte division aide à empêcher les fuites de mémoire (memory leaks) spécifiques à Godot. C'est pour cela que l'EventBus utilise des `WeakReference` : si Godot supprime un nœud (`QueueFree()`), le Core ne le gardera pas en mémoire par erreur.

## Exemple Pratique

**Mauvaise Approche :**
Un ennemi (`Soldier.cs`) qui, dans sa boucle `_PhysicsProcess`, calculerait lui-même ses points d'attaque en lisant dans une base de données, gèrerait son temps de recharge avec un `Godot.Timer`, et appellerait `GetTree().ChangeScene()` s'il a tué le joueur.

**Bonne Approche (IslandSurvivor) :**
*   **Core (`StatTracker.cs`) :** Calcule que la base d'attaque vaut `BaseAttackValue` modifiée par `StatType.Attack`.
*   **Godot (`AttackController.cs`) :** Utilise la vitesse de déplacement Godot et les entrées. En cas de collision (`Area2D`), il envoie `TakeDamage` à la cible en demandant la valeur de dommage au `StatManager` (Core).
*   **Godot (`Soldier.cs`) :** Si la cible meurt, il joue l'animation `Die` et publie `EnemyKilledEvent` via l'EventBus. Le `ScoreTracker` (Core) reçoit l'événement et augmente le score pur. Le `ScoreManager` (Godot) lit l'EventBus et dit à l'UI Godot de changer le texte à l'écran.