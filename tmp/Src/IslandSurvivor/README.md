# Projet IslandSurvivor (Client de Jeu Godot)

## 📌 Responsabilité Unique
Ce répertoire contient l'intégralité du client de jeu développé sous Godot 4. Il est responsable de la boucle de gameplay locale, du rendu visuel, de la physique, de l'audio et des interactions joueur.

## ✅ Composants Autorisés
- Scripts de scènes (`.tscn`) et leurs classes C# (`Nodes/`, `Classes/`)
- Contrôleurs de jeu, Managers locaux (`Managers/`)
- Ressources graphiques et sonores (`Assets/`)
- Appels aux APIs distantes (de manière asynchrone pour ne pas bloquer le thread principal).

## 🚫 Dépendances Interdites
- **Infrastructure & BDD** : Il est strictement interdit d'interroger directement la base de données SQL ou de référencer le projet `Infrastructure`.
- **Logique Mathématique Pure** : Éviter de dupliquer les algorithmes mathématiques complexes (ex: progression de niveau) qui doivent se trouver dans le `Core`.

## 💡 Conseils pour l'équipe (et l'IA)
- Isolez la logique visuelle de la logique métier. Utilisez le pattern d'événements (`EventBus` fourni par le `Core`) pour faire communiquer l'état du jeu et les réactions visuelles.
- Lors de l'utilisation de l'attribut `[Tool]`, commencez toujours vos méthodes de cycle de vie par `if (Engine.IsEditorHint()) return;` pour éviter d'exécuter la logique de jeu dans l'éditeur Godot.

- **Consommation du Core** : Le jeu orchestre l'espace-temps, mais délègue la validation mathématique des états (Score, XP, passage de niveau) au projet `Core` via l'injection de services ou l'EventBus.
