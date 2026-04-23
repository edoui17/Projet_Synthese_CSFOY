# Forge's Journal - IslandSurvivor Technical Learnings

Ce document centralise les décisions architecturales, les particularités de Godot 4.6.1 et les patrons de synchronisation N-Tier pour le projet IslandSurvivor.

---

## Contexte & Architecture
- **Moteur :** Godot 4.6.1 (.NET 8 / C#)
- **Architecture :** N-Tier (Core, API, Client, Infrastructure, Web)
- **Genre :** Roguelike
- **Principe Fondamental :** Séparation stricte Core/Client. Le projet Core est indépendant de Godot. Les domaines purs ne traitent pas d'assets (ex: Texture2D). Les items utilisent des chemins (string IconPath) que le client Godot résout en images.

---

## Conventions de Codage (Strict Enforcements)
Pour maintenir une cohérence absolue à travers les projets .NET 8, les règles suivantes sont appliquées :

1. **Typage :** Le mot-clé var est strictement interdit. Tous les types doivent être explicites (ex: List<string> items = new List<string>()).
2. **Isolation des Classes :** Chaque classe (même les variantes génériques comme WeakEvent vs WeakEvent<T>) doit résider dans son propre fichier.
3. **Ordre des Membres :**
   - **1. Variables membres :** Champs privés commençant par m_ tout en haut.
   - **2. Constructeurs :** Immédiatement après les variables membres.
   - **3. Propriétés :** Immédiatement après les constructeurs.
   - **4. Méthodes :** À la fin de la classe.

---

## Gestion des Événements & Interopérabilité (Bridge Pattern)
**Problématique :** Coupler la logique métier aux signaux Godot lie le Core au moteur et complique les tests unitaires.

**Résolution (Le Bridge Pattern) :**
- **Core :** Implémentation d'un pattern WeakEvent (utilisant WeakReference). Cela permet des tests via xUnit et évite les fuites de mémoire sans nécessiter de désabonnement explicite strict lors de la suppression d'objets.
- **Godot (Proxy) :** Le SignalManager de Godot (Autoload) sert de "colle". Il écoute les WeakEvents du Core et les relaie via des [Signal] natifs.
- **Avantage :** L'inspecteur Godot peut réagir aux événements (VFX, sons, UI) via les signaux, tandis que la logique reste testable et pure.

---

## Systèmes de Jeu

### 1. Gestion des Statistiques (Entity Stat System)
- **StatTracker (Core) :** Gère les calculs complexes (scaling, caps) et déclenche les WeakEvents.
- **EntityStats (Godot Resource) :** Utilisé comme un [GlobalClass] immuable. C'est un simple template de configuration injecté au StatTracker lors du _Ready().
- **Découplage :** Les améliorations de stats émettent StatUpgradePurchased pour éviter de coupler les scripts d'interaction directement au StatManager.

### 2. Génération Procédurale de Map (Approche Hybride)
- **Logique :** L'interface IMapGenerator est dans le Core, mais l'implémentation GodotIslandGenerator est dans le projet Godot pour utiliser FastNoiseLite.
- **Élévation & Navigation :**
  - Utilisation de constantes string ("Water", "Ground") converties en coordonnées Atlas Vector2I.
  - Algorithme BFS pour détecter les bordures de plateaux (falaises) et garantir l'accès via des tuiles "Escaliers".
- **Rendu :** MapRenderer utilise SetCellsTerrainConnect() en batch. Les effets de mousse (Foam) sont gérés par un TileMapLayer superposé avec un tri de profondeur (Z-index) sous le sol.

### 3. IA Passive (Le Mouton - US 4.4)
- **Découplage :** La logique de fuite (Flee) et les timers tournent dans le SheepController (C# pur). Le nœud Godot (CharacterBody2D) transmet uniquement le delta.
- **Loot :** À la mort, le script appelle SignalManager.Instance.EmitMaterialDestroyed(...) pour notifier l'inventaire.

---

## Persistance & Navigation
- **Transition Différée :** Le changement de scène est découplé de l'UI. Le bouton UI active un "Portail" après paiement, et la transition se fait lors de l'interaction physique du joueur avec celui-ci.
- **Sauvegarde N-Tier :** Le NavigationManager (Autoload) intercepte le changement de scène pour appeler GodotSaveService, sérialisant l'état de l'IInventoryManager et du SessionState avant le ChangeSceneToFile.
- **Chemins :** Éviter les chemins relatifs (../../). Utiliser GetTree().Root.GetNodeOrNull(...) pour garantir la stabilité face aux changements de hiérarchie.

---

## Godot Quirks & Physique
- **Collision Layers vs Masks :**
  - **Layer :** Ce que je suis.
  - **Mask :** Ce que je détecte.
- **Configuration IslandSurvivor :** Le Player (Layer 3) ne collisionne pas physiquement avec les objets interactifs (Layer 2), mais son Area2D de détection possède un Mask 2.
- **Signaux Area2D :** Pour émettre body_exited, la propriété monitoring doit être à true.
- **Singletons :** Pour maintenir l'état entre les scènes, InventoryNode utilise l'Autoload Godot combiné à des instances statiques pour son implémentation .NET.

---

## Synchronisation & Meta-Progression (US 8.1)
**Problématique :** Assurer la persistance des données (Stats, Config, Inventaire) de manière sécurisée et optimisée.

**Architecture de Synchronisation :**
- **Sécurité :** Système de "Session Token" (API Key par utilisateur). Le login retourne un token qui doit être inclus dans le corps des requêtes POST ou dans les headers pour les GET.
- **Optimisation (Consolidation) :**
  - `GET /api/player/profile` : Récupère l'intégralité du profil (Joueur, Stats, Inventaire, Config) en un seul appel au lancement.
  - `POST /api/player/sync` : Envoie l'état complet du jeu pour une sauvegarde atomique.
- **Granularité :** Des endpoints individuels (Stats, Inventory) permettent des mises à jour incrémentales durant le gameplay sans surcharger le réseau.
- **Mapping :** Mapping manuel systématique entre les `Entities` (Infrastructure) et les `Domain Models` (Core) pour garantir l'indépendance des couches.