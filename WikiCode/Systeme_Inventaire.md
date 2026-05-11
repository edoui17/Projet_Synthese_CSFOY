# Système de Gestion d'Inventaire (US 4.3)

Ce document explique comment l'inventaire est conçu dans IslandSurvivor, en respectant rigoureusement l'architecture N-Tier. L'objectif est d'avoir une gestion purement métier (C#) tout en permettant à Godot d'y accéder de n'importe où, même après un changement de scène.

## 1. La Couche Métier (Core)

Le Core (`Src/Core`) contient la véritable logique de l'inventaire, sans dépendre des classes de Godot (`Node`, `Texture2D`, etc.). Cela signifie que l'inventaire pourrait théoriquement être utilisé dans un serveur distant ou dans un autre moteur de jeu sans modification.

### Les Modèles de Données
Pour gérer l'inventaire, deux modèles principaux existent dans `Src/Core/Domain/` :

1. **`ResourceItem`** : Représente les propriétés de base d'un objet (ID, Nom, Type).
   - *Note technique* : Plutôt que de stocker une image Godot (`Texture2D`), on stocke un chemin absolu sous forme de texte (`string IconPath = "res://..."`). C'est l'interface Godot qui devra charger l'image.
2. **`InventorySlot`** : Représente un emplacement dans l'inventaire. Il combine un `ResourceItem` avec une quantité (`int Quantity`).

### `InventoryManager`
C'est la classe métier (`Src/Core/Managers/InventoryManager.cs`) qui contient un dictionnaire (`Dictionary<string, InventorySlot>`).
Elle fournit des méthodes sécurisées :
- `AddMaterial(ResourceItem p_item, int p_amount)` : Ajoute ou incrémente un slot.
- `RemoveMaterial(string p_itemId, int p_amount)` : Soustrait la quantité et supprime le slot s'il tombe à 0 ou moins.
- `GetAllSlots()` : Retourne une liste en lecture seule pour que la future UI (Menu) puisse s'afficher correctement.

## 2. Le Pont Godot (`InventoryNode`)

Pour que Godot puisse interagir avec `InventoryManager`, une classe spéciale `InventoryNode` (`Src/IslandSurvivor/Globals/InventoryNode.cs`) a été créée dans la couche Godot (Client).

### Fonctionnalités Clés :
1. **Singleton (Autoload)** :
   `InventoryNode` est pensé pour être ajouté dans les paramètres du projet Godot comme "Autoload".
   - Dans `_EnterTree()`, il s'assure qu'il est l'unique instance (`Instance = this`).
   - Cela lui permet de **survivre aux changements de scènes**. Si le joueur passe de la forêt à la mine, l'inventaire ne sera pas effacé car ce nœud global restera en vie.

2. **Écouteur d'Événements et Injection de Dépendance** :
   Dans `_Ready()`, `InventoryNode` récupère l'instance unique de `InventoryManager` et l'`EventBus` via le `ServiceRegistry`. Il s'abonne ensuite à l'événement `MaterialDestroyedEvent` via l'**EventBus**.

### Le Cycle Complet
1. Un joueur détruit un arbre dans Godot.
2. Le script de l'arbre instancie un `ResourceItem` et publie un événement sur l'EventBus : `m_eventBus.Publish(new MaterialDestroyedEvent(item, 3))`.
3. L'**EventBus** transmet cet événement à tous ses abonnés (ici l'`InventoryNode`).
4. Le **InventoryNode** (Godot) reçoit l'événement.
5. Il appelle `m_inventoryManager.AddMaterial(item, 3)` pour mettre à jour la logique Core.
6. L'`InventoryManager` (Core) peut alors publier un `InventoryChangedEvent` pour indiquer qu'un objet a été ajouté.
7. L'inventaire est à jour et prêt à être persisté via le [Persistence System](./Persistence_System.md).

### Synchronisation (US 8.1)
L'inventaire est automatiquement synchronisé avec l'API :
- **Chargement** : Au lancement (`GET /api/player/profile`).
- **Sauvegarde** : Via les endpoints de synchronisation (`POST /api/player/sync` ou `/api/inventory/upsert`).
