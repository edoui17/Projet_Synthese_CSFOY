# IslandSurvivor - Journal d'Utilisation de l'IA (KH)

Ce document retrace l'interaction entre Kevin Houle et les assistants IA (Forge/Atlas) pour documenter le processus de conception et le rôle de l'IA dans le projet.

---

## Historique d'Utilisation

### 2026-03-29 - Configuration de l'Assistant "Forge"
- **Requête :** Créer un prompt système complet pour un assistant IA (Forge) afin d'aider au développement du projet Roguelike "IslandSurvivor".
- **Contribution de l'IA :** Définition des frontières architecturales (N-Tier), des conventions de codage (préfixe `m_` pour les champs et `p_` pour les paramètres) et établissement d'un processus de journalisation rigoureux.
- **Raisonnement :** Choix d'une approche basée sur une Persona (Forge) pour garantir la cohérence du style de code et l'intégrité architecturale, en séparant la logique partagée (Core) de la présentation (Godot/Blazor).

### 2026-03-29 - [User Story 5.3] Gestionnaire de signaux global (SignalManager)
- **Requête :** Implémenter un SignalManager global (Autoload) pour assurer une communication fluide entre les systèmes de jeu sans fuites de mémoire.
- **Contribution de l'IA :** Conception et implémentation d'un pattern `WeakEvent` en C# (Core) utilisant `WeakReference` pour permettre au Garbage Collector de nettoyer automatiquement les nœuds Godot détruits. Création de `ISignalManager` et de ses implémentations. Écriture de tests xUnit. Refactorisation pour séparation stricte des fichiers et ordre des membres.
- **Raisonnement :** Choix d'événements C# purs avec `WeakReference` pour découpler la logique `Core` du moteur Godot et permettre des tests unitaires complets sans environnement Godot.

### 2026-03-30 - [User Story 5.1] Implémenter le gestionnaire de statistiques
- **Requête :** Implémenter le gestionnaire de statistiques d'entité (Santé, Attaque, Chance, Vitesse) en suivant l'architecture N-Tier.
- **Contribution de l'IA :** Construction d'un tracker de stats basé sur des Enum dans `/Src/Core`. Implémentation des classes `StatTracker` et `Stat`. Dans Godot, exposition d'une config `[GlobalClass]` via `EntityStats` (Resource), chargée par un nœud `StatManager` via le pattern Bridge, réémettant les signaux `WeakEvent` vers des `[Signal]` natifs de Godot.
- **Raisonnement :** L'approche par "Pont" (Bridge) permet au Core de rester ignorant de Godot tout en restaurant les fonctionnalités de l'éditeur Godot.

### 2026-04-09 - [US 3.2 : Implémenter la navigation entre les îles]
- **Requête :** Ajouter un système de navigation, suivre l'île actuelle, gérer les coûts de transition et la persistance.
- **Contribution de l'IA :** 1. **Core :** Ajout de `IslandDestination`, extension de `SessionState`. Logique de déduction de ressources dans `NavigationService`.
    2. **Godot Bridge :** Extension de `SignalManager`. 
    3. **UI/Interaction :** Création de `NavigationMenu.tscn` et `PortalInteraction.cs`.
    4. **Intégration :** `NavigationManager` (autoload) gère le changement de scène et la sérialisation via `ISaveService` avant la transition.
- **Raisonnement :** Le joueur interagit avec un portail physique pour déclencher la transition, gardant le gameplay immersif. L'interception de la transition dans le `NavigationManager` assure que l'inventaire et l'état de la session sont sauvegardés sur le disque.

### 2026-04-11 - [Feature: Gestion des Layers de Collision]
- **Requête :** Configurer les layers de collision pour corriger l'absence d'interaction entre le Player et le building_node.
- **Contribution de l'IA :** - Configuration des layers 2D : Environnement, Interaction, Player, Combat, Ressource.
    - Correction des masques de collision dans `Player.tscn` et `building_node.tscn`.
    - Mise à jour des ressources (Gold, Rock, Portal, etc.).
    - Documentation dans `WikiCode/CollisionLayers.md`.
- **Raisonnement :** La séparation des layers (ex: Interaction vs Corps physique) optimise la détection physique et évite les faux positifs ou les conflits entre la logique de combat et de terrain.

### [User Story 5.2 : Implémenter le système de point]
- **Requête :** Accumulation du score, validation des valeurs positives, signal de mise à jour UI et persistance du High Score.
- **Contribution de l'IA :** 1. **Core :** `SessionState` pour les données mutables de runtime. Création de `ISaveService` (Inversion de dépendance). 
    2. **Managers :** `ScoreTracker` gérant la logique de validation et de persistance.
    3. **Godot :** `ScoreManager` agissant comme pont entre le Core et l'UI Godot. Tests unitaires avec Moq.
- **Raisonnement :** Isolation de l'état "Live" pour éviter de muter les ressources Godot (templates) au runtime, simplifiant ainsi la sérialisation JSON.

### [US 3.1 : Génération Procédurale (Map, Elévation, Splash)]
- **Requête :** Implémenter la génération d'îles (Plateaux, Falaises, Escaliers) et les transitions visuelles (écume).
- **Contribution de l'IA :** - Création de `IMapGenerator` dans le Core. Implémentation de `GodotIslandGenerator` avec `FastNoiseLite` et algorithme BFS.
    - Logique de "multi-pass" pour l'élévation (bordures de falaises).
    - Ajout automatique de tuiles d'écume (`FoamWaterTileMap`) par vérification de voisinage dans Godot.
    - Création du `MapRenderer.cs` pour traduire les données du Core en `SetCellsTerrainConnect`.
- **Raisonnement :** La logique visuelle (écume) reste dans Godot, tandis que la structure de la map est générée par le Core sous forme de données neutres (strings), respectant la séparation N-Tier.

### [Bugfix: NavigationMenu non visible]
- **Requête :** Le menu de navigation n'apparaît pas lors de l'interaction.
- **Contribution de l'IA :** Modification de `MaterialsMenuPlanner.cs` pour utiliser un chemin de nœud robuste (`GetTree().Root...`) et appel de `OpenMenu()` au lieu de simplement changer la visibilité.
- **Raisonnement :** Forcer uniquement `Visible = true` ne déclenchait pas la génération dynamique des boutons. L'appel explicite à la méthode dédiée garantit l'initialisation de l'UI.
