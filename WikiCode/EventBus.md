# Système d'Événements (EventBus)

## Vue d'ensemble

Le système **EventBus** est une infrastructure clé de l'architecture N-Tier de *IslandSurvivor*. Il implémente le patron de conception **Publication-Souscription (Pub-Sub)** pour permettre une communication découplée entre les différents gestionnaires (Managers) et systèmes du jeu.

Placé dans `Src/Core/Services`, l'EventBus est considéré comme un service transversal (cross-cutting concern). Il permet par exemple au `StatsManager` de réagir aux événements de jeu (comme la récolte de ressources) sans avoir besoin d'une référence directe vers le système source.

## Principes Fondamentaux

1. **Communication Exclusive du Core** : Le Core ne communique vers l'extérieur (ou entre ses propres systèmes internes) **que** par l'EventBus. Les `WeakEvents` et les événements C# classiques ne sont plus utilisés dans les Managers du Core pour la communication globale.
2. **Découplage** : Les émetteurs (Publishers) n'ont pas connaissance des récepteurs (Subscribers), et vice-versa.
3. **Typage Fort** : L'EventBus utilise des événements génériques fortement typés (souvent des records POCOs) garantissant la sécurité à la compilation via l'interface marqueur `IEvent`.
4. **Sécurité Mémoire (WeakReference)** : Les souscriptions utilisent un wrapper (`WeakAction<T>`) autour des délégués (`Action<T>`). Cela garantit que si l'objet abonné (par exemple, un nœud Godot ou un menu temporaire) est détruit, l'EventBus ne maintiendra pas l'objet en vie, évitant ainsi les fuites de mémoire.
5. **Désabonnement Explicite** : Bien que le système gère les références faibles, l'interface `IEventBus` fournit une méthode `Unsubscribe`. Celle-ci est **obligatoire** pour les objets à courte durée de vie (ex: menus d'interface utilisateur) afin qu'ils cessent de réagir aux événements dès qu'ils sont cachés ou désactivés, avant même le passage du ramasse-miettes (Garbage Collector).

## Implémentation Core

### Interface Marqueur `IEvent`
Tous les événements envoyés via l'EventBus doivent implémenter cette interface vide.
```csharp
namespace Core.Interfaces;

public interface IEvent
{
    // Marker interface for EventBus type safety.
}
```

### Interface `IEventBus`
```csharp
using System;

namespace Core.Interfaces;

/// <summary>
/// Interface for a generic Event Bus enabling decoupled communication.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Subscribes a callback to a specific event type.
    /// </summary>
    void Subscribe<T>(Action<T> p_callback) where T : IEvent;

    /// <summary>
    /// Unsubscribes a callback from a specific event type.
    /// </summary>
    void Unsubscribe<T>(Action<T> p_callback) where T : IEvent;

    /// <summary>
    /// Enqueues an event to be published during the next ProcessEvents call.
    /// </summary>
    void Publish<T>(T p_event) where T : IEvent;

    /// <summary>
    /// Processes and dispatches all queued events to their subscribers.
    /// </summary>
    void ProcessEvents();
}
```

## Utilisation de l'EventBus

### Configuration (Godot Autoload)
Puisque le `EventBus` utilise une file d'attente concurrente (`ConcurrentQueue<Action>`) pour éviter la récursion infinie ou les blocages de synchronisation (Option B), il **doit** être traité à chaque frame. Le script Autoload `ServiceRegistry` de Godot se charge d'appeler `ProcessEvents()`.

```csharp
public partial class ServiceRegistry : Node
{
    public IEventBus EventBus { get; private set; } = null!;

    public override void _EnterTree()
    {
        // ... initialisation ...
        EventBus = new EventBus();
    }

    public override void _Process(double delta)
    {
        EventBus?.ProcessEvents();
    }
}
```

### 1. Créer un Événement
Pour créer un nouvel événement, définissez un `record` ou une `class` implémentant `IEvent`. Ces objets doivent être placés dans le dossier `Src/Core/Events/`. Utilisez `record` pour des événements simples de transfert de données et `class` pour des événements nécessitant plus de logique interne (comme `LevelChangedEvent` ou `ExperienceGainedEvent`).

```csharp
using Core.Interfaces;

public record MaterialDestroyedEvent(Core.Domain.ResourceItem Item, int MaterialQuantity) : IEvent;
```

### 2. S'abonner à un Événement (Subscribe)
Un système intéressé par l'événement s'abonne via l'EventBus.

```csharp
public class StatsManager
{
    private readonly IEventBus m_eventBus;

    public StatsManager(IEventBus p_eventBus)
    {
        m_eventBus = p_eventBus;
        m_eventBus.Subscribe<MaterialDestroyedEvent>(OnMaterialDestroyedEvent);
    }

    private void OnMaterialDestroyedEvent(MaterialDestroyedEvent p_event)
    {
        // Traiter l'événement
        Console.WriteLine($"Récolté {p_event.MaterialQuantity} de {p_event.Item.Id}");
    }
}
```

### 3. Publier un Événement (Publish)
Le système source publie l'événement lorsqu'une action se produit.

```csharp
public class ResourceNode
{
    private readonly IEventBus m_eventBus;

    public void DestroyMaterial(Core.Domain.ResourceItem item, int quantity)
    {
        // ... logique de destruction ...

        m_eventBus.Publish(new MaterialDestroyedEvent(item, quantity));
    }
}
```

### 4. Se désabonner (Unsubscribe) - Obligatoire pour les objets temporaires
Lorsqu'un système ou une UI n'a plus besoin de recevoir d'événements, ou juste avant sa destruction, il doit se désabonner explicitement.

```csharp
public class TemporalUI : Godot.Control
{
    private readonly IEventBus m_eventBus;

    public override void _Ready()
    {
        m_eventBus.Subscribe<MaterialDestroyedEvent>(OnMaterialDestroyedEvent);
    }

    public override void _ExitTree()
    {
        m_eventBus.Unsubscribe<MaterialDestroyedEvent>(OnMaterialDestroyedEvent);
    }
}
```

## Thread-Safety

L'implémentation `EventBus` gère de manière basique la sécurité des threads (Thread-Safety) en utilisant des blocs `lock` autour du dictionnaire des souscriptions (`Dictionary<Type, List<object>> m_subscriptions`). Les abonnés sont stockés sous forme d'objets `WeakAction<T>` pour éviter les fuites de mémoire.

Lors de la publication (`Publish`), les événements sont d'abord placés dans une `ConcurrentQueue<Action>` (`m_eventQueue`). Ensuite, lors de l'appel à `ProcessEvents()` (généralement dans la boucle de rendu principale `_Process`), ces actions sont dépilées et exécutées en appelant `DispatchEvent()`.

Dans `DispatchEvent()`, une copie (snapshot) des souscripteurs actifs est créée en nettoyant les références mortes sous un bloc `lock`. Ensuite, l'invocation des événements (l'appel aux callbacks) se fait en dehors du bloc `lock`. Cette approche permet d'éviter les interblocages (deadlocks) et les exceptions de collection modifiée si un souscripteur publie un autre événement ou modifie ses souscriptions pendant le traitement de l'événement.
