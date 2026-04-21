# Audit Phase 2 - Injection et Bridge Pattern

## 1. Test d'intégration (DI) via ServiceRegistry
- **Création du `ServiceRegistry`:** Un Autoload Godot a été créé pour centraliser l'instanciation et l'injection de tous les Core Managers de manière N-Tier (`InventoryManager`, `ShopManager`, `StatTracker`, `ScoreTracker`, `NavigationService`, et `SignalManagerCore`).
- **Suppression du couplage fort:** `InventoryNode`, `ScoreManager`, `StatManager`, et `MaterialsMenuPlanner` instancièrent tous les Managers manuellement en utilisant `new Manager()`. Cela posait un problème de persistance (chaque scène récréait l'état). Tous récupèrent désormais leurs références de `ServiceRegistry.Instance`.
- **GameManager et Map Generation:** L'Autoload `GameManager` a été retiré de `project.godot` et le fichier a été supprimé puisque la génération de map procédurale est mise en pause et n'avait pas besoin de polluer l'injecteur global de dépendances.

## 2. Conformité du Bridge Pattern (Signaux Godot)
- **Le problème initial:** Le `SignalManager` Godot exposait directement les `WeakEvent` du C# pur de `SignalManagerCore`. Les nœuds Godot utilisaient `.AddListener()`, créant une confusion entre le cycle de vie Godot et le cycle de vie mémoire du Core.
- **La correction (Native Signals):** Le `SignalManager` agit désormais comme un vrai "Bridge". Il s'abonne aux `WeakEvents` de `SignalManagerCore` (avec des expressions lambdas ou handlers) et ré-émet des signaux natifs Godot (`[Signal] public delegate void...`).
- **Adaptation des Nœuds:** Les nœuds Godot s'abonnent désormais aux signaux Godot normaux via `+=` (ex: `SignalManager.Instance.MaterialDestroyed += OnMaterialDestroyed`). Godot gère le cycle de vie via son arborescence de scène, éliminant les risques de "Lapsed Listener Problem" lié aux délégués si on navigue entre les scènes sans les `WeakEvents` côté Godot.

## 3. Amélioration de la stabilité
Ces refontes permettent de naviguer de scènes en scènes :
1. **L'état (Inventaire, Score, Stats)** est conservé globalement via le `ServiceRegistry` sans perte de progression ni d'effet de bord lors du rechargement d'un niveau.
2. **Aucune fuite mémoire** n'est possible avec des événements natifs de Godot qui lient l'abonnement à l'existence du noeud (ou qu'on nettoie dans `_ExitTree`).

## 4. Phase 4 - Documentation et Clôture
- **Mise à jour du Wiki (`WikiCode/`)** : Les documentations techniques (`SignalManager.md`, `SystemeNavigation.md`, `ScoreManager.md`, `Systeme_Inventaire.md`) ont été mises à jour pour refléter correctement la nouvelle architecture (Injection de Dépendance avec `ServiceRegistry` et Bridge Pattern avec les Signaux Natifs Godot).
- **Orientation Utilisateur** : Les exemples de code illustrent désormais clairement aux développeurs comment s'abonner avec `+=` sans briser l'isolation du Core.
- **Clôture** : Ces mises à jour scellent l'Itération 0.1 en établissant une référence solide et exempte de dette architecturale pour le reste de l'équipe.
