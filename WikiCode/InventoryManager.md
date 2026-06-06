# Inventory Manager Architecture

Le `InventoryManager` est le cœur logique de la gestion des inventaires dans *IslandSurvivor*. Situé dans le projet `Src/Core`, il respecte rigoureusement l'architecture N-Tier "Accountant vs. Orchestrator".

En tant que "Comptable", le `InventoryManager` ne manipule que des données brutes et ne dépend d'aucune fonctionnalité du moteur Godot (pas de `Node`, pas d'interfaces graphiques ou de `Texture2D`).

## 1. Modèles de Données Associés

L'inventaire repose sur deux entités fondamentales :
- **`ResourceItem`** : Représente la définition d'un objet (Id, Nom, Type, et un chemin textuel `IconPath` au lieu d'une image Godot, pour rester agnostique).
- **`InventorySlot`** : Associe un `ResourceItem` à une quantité (`Quantity`). C'est l'unité de stockage dans le dictionnaire interne du manager.

## 2. Fonctionnalités Principales

La classe `InventoryManager` gère un dictionnaire d'emplacements (`Dictionary<string, InventorySlot>`) et expose les méthodes suivantes :

### `AddMaterial(ResourceItem p_item, int p_amount)`
Ajoute une quantité spécifique d'un objet. Si l'objet existe déjà, sa quantité est incrémentée. Autrement, un nouveau `InventorySlot` est créé.
Chaque modification déclenche la publication d'un événement `InventoryChangedEvent` sur l'EventBus.

### `RemoveMaterial(string p_itemId, int p_amount)`
Soustrait la quantité spécifiée de l'objet correspondant à `p_itemId`. Si la quantité d'un `InventorySlot` atteint 0 (ou moins), l'emplacement est retiré de l'inventaire.

### `GetMaterialCount(string p_itemId)`
Renvoie la quantité possédée d'un objet donné, de manière sécurisée (retourne `0` si l'objet n'existe pas).

### `GetAllSlots()`
Renvoie une liste en lecture seule (`IReadOnlyList<InventorySlot>`) permettant à l'interface (UI) de s'afficher sans pouvoir modifier directement l'état interne.

### Initialisation et Nettoyage
- **`InitializeInventory`** : Remplit l'inventaire à partir d'une liste existante (`InventoryEntry`), généralement appelée lors du chargement du profil via l'API.
- **`ClearInventory`** : Vide complètement l'inventaire, en notifiant chaque emplacement vidé via `InventoryChangedEvent`.

## 3. Écoute Active des Événements (EventBus)

L'`InventoryManager` écoute l'EventBus pour réagir automatiquement aux événements métiers globaux sans couplage avec l'interface :
- `NavigationRequestedEvent` : Vérifie que le joueur possède les ressources (`MEAT`, `WOOD`, `ROCK`, `GOLD`) nécessaires pour payer le coût du voyage et les déduit, émettant au besoin `NavigationApprovedEvent` ou `NavigationRejectedEvent`.
- `PurchaseAttemptedEvent` : Similaire à la navigation, vérifie la disponibilité pour un achat et l'approuve/refuse via `TransactionResultEvent`.
- `ResourceHarvestedEvent` : Appelle `AddMaterial` avec un objet générique si cet événement est capté (bien que ce soit habituellement relayé autrement, voir [Système d'Inventaire](./Systeme_Inventaire.md)).
- `ProfileLoadedEvent` : Initialise l'inventaire complet dès réception des données de profil sauvegardées.

## Lien avec l'Orchestrateur

La logique pure vit ici, mais pour être utilisée dans le moteur de jeu de manière persistante, elle est encapsulée par le noeud global Godot `InventoryNode`. Référez-vous à [Système d'Inventaire (US 4.3)](./Systeme_Inventaire.md) pour les détails d'intégration avec le Client.
