# Forge's Journal - IslandSurvivor Technical Learnings

Ce document centralise les décisions architecturales, les particularités de Godot 4.6.1 et les patrons de synchronisation N-Tier pour le projet IslandSurvivor.

---

## 1. Contexte & Architecture Global
- **Moteur :** Godot 4.6.1 (.NET 8 / C#)
- **Architecture :** N-Tier (Core, API, Client, Infrastructure, Web)
- **Principe Fondamental :** Séparation stricte Core/Client. 
    - Le projet **Core** est indépendant de Godot et utilise `System.Numerics` pour rester agnostique.
    - Le **Client** (IslandSurvivor) utilise les APIs natives (`Godot.Vector2`, `MoveAndSlide()`) pour la performance et la cohésion.
- **Audit de Domaine (Phase 1) :** Les entités liées à la boucle de jeu (`SheepController`, `HealthComponent`) résident dans le Client (`Logic/Entities/`). Garder du code spécifique aux visuels dans le Core violerait l'objectif d'une couche pure partageable avec une API ASP.NET.

---

## 2. Conventions de Codage (Strict Enforcements)
Pour maintenir une cohérence absolue, les règles suivantes sont appliquées :

- **Typage :** Le mot-clé `var` est strictement interdit. Tous les types doivent être explicites (ex: `List<string> items = new List<string>()`).
- **Isolation des Classes :** Chaque classe doit résider dans son propre fichier.
    - *Nuance d'Encapsulation* : Les classes privées imbriquées (ex: `WeakDelegate` dans `WeakEvent`) ne doivent pas être dégroupées pour préserver l'atomicité du système.
- **Ordre des Membres :**
    1. **Variables membres :** Champs privés commençant par `m_`.
    2. **Constructeurs :** Immédiatement après les variables.
    3. **Propriétés :** Immédiatement après les constructeurs.
    4. **Méthodes :** À la fin de la classe.

---

## 3. Gestion des Événements & Bridge Pattern
**Problématique :** Coupler la logique métier aux signaux Godot lie le Core au moteur.

- **Core (WeakEvent) :** Implémentation utilisant `WeakReference`. 
    - *Découverte* : Utilise `.AddListener()` et `.RemoveListener()` au lieu des opérateurs standards `+=` / `-=`.
- **Godot (Proxy/Bridge) :** Le `SignalManager` (Autoload) écoute les `WeakEvents` du Core et les relaie via des `[Signal]` natifs.
- **Injection .NET 8 :** Les signaux Godot ne supportent pas les classes C# pures. Le Bridge doit décomposer les objets complexes (ex: `IslandDestination`) en types primitifs (`string`, `int`) lors de la ré-émission.

---

## 4. Systèmes de Jeu & UI (Client Godot)

### UI et Placement
- **CanvasLayer :** Placer les menus interactifs globaux (`NavigationMenu`) dans un `CanvasLayer` pour éviter qu'ils ne soient masqués par des éléments du monde ou affectés par la caméra.
- **Filtres d'entrée :** Les éléments UI profonds dans l'arbre `Node2D` peuvent voir leurs inputs absorbés par des collisions ou filtres de souris.

### IA et Mouvement (US 4.4 & 5.4)
- **MovementController :** Maintenu dans le projet Client. Utiliser le moteur physique de Godot (`MoveAndSlide`) est plus performant que de recréer un solveur de collision dans le Core.
- **Loot & Destruction :** À la mort, les entités appellent `SignalManager.Instance.EmitMaterialDestroyed(...)` et utilisent `QueueFree()` pour garantir la libération de la mémoire.

### Héritage de Scènes
- Lors du refactoring vers des scènes héritées, les scènes enfants nécessitent un index pathing spécifique pour injecter des nœuds (ex: Map dans `MapContainer`) sans briser l'architecture de base.

---

## 5. Navigation & Persistance

### Architecture de Navigation
- **NavigationManager :** Centralise les transitions via `SceneLoadingManager`.
- **Auto-Active Portal (Retour) :** Les portails hors du `PlayerHub` s'activent automatiquement au `_Ready()` via `GetTree().CurrentScene.SceneFilePath` pour servir de point de retour permanent vers le Home.
- **Localisation du Joueur :** Le joueur est extrait des maps de base pour être placé directement dans les scènes `Level{X}.tscn`.

### Gestion des Sauvegardes
- **Chemins de fichiers :** Pour sortir de `res://` vers la racine de la solution (`/Save/`), utiliser `ProjectSettings.GlobalizePath("res://../../Save/")`.
- **API :** Prioriser `DirAccess` et `FileAccess` de Godot pour respecter les répertoires virtuels (`user://`).

---

## 6. Maintenance & Quirks Techniques
- **Audit Phase 3 (Ressources) :** - Les marqueurs de conflits Git (`<<<<<<< HEAD`) corrompent le parsing des fichiers `.tres` et `.tscn`. Correction manuelle requise.
    - Préfixer les paramètres des `delegate` de signaux par `p_` pour la conformité.
- **Physique :** - **Layer** = Ce que je suis (Player: Layer 3).
    - **Mask** = Ce que je détecte (Interactibles: Mask 2).