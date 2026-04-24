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
namespace Core.Interfaces;

public interface IEventBus
{
    void Subscribe<T>(Action<T> p_callback) where T : IEvent;
    void Unsubscribe<T>(Action<T> p_callback) where T : IEvent;
    void Publish<T>(T p_event) where T : IEvent;
    void ProcessEvents();
}
```

## Utilisation de l'EventBus

### Configuration (Godot Autoload)
Puisque le `EventBus` utilise une file d'attente concurrente (`ConcurrentQueue<Action>`) pour éviter la récursion infinie ou les blocages de synchronisation (Option B), il **doit** être traité à chaque frame. Un script Autoload dans Godot doit appeler `ProcessEvents()`.

```csharp
public partial class EventBusAutoload : Node
{
    private IEventBus m_eventBus;

    public override void _Ready()
    {
        // Injecter ou récupérer le bus
        m_eventBus = ServiceRegistry.Get<IEventBus>();
    }

    public override void _Process(double delta)
    {
        m_eventBus.ProcessEvents();
    }
}
```

### 1. Créer un Événement
Pour créer un nouvel événement, définissez un record ou une classe implémentant `IEvent`. Ces objets doivent être placés dans le dossier `Src/Core/Events/`.

```csharp
using Core.Interfaces;

public record ResourceHarvestedEvent(string ResourceId, int Amount) : IEvent;
```

### 2. S'abonner à un Événement (Subscribe)
Un système intéressé par l'événement (ex: `StatsManager`) s'abonne via l'EventBus.

```csharp
public class StatsManager
{
    private readonly IEventBus m_eventBus;

    public StatsManager(IEventBus p_eventBus)
    {
        m_eventBus = p_eventBus;
        m_eventBus.Subscribe<ResourceHarvestedEvent>(OnResourceHarvested);
    }

    private void OnResourceHarvested(ResourceHarvestedEvent p_event)
    {
        // Traiter l'événement
        Console.WriteLine($"Récolté {p_event.Amount} de {p_event.ResourceId}");
    }
}
```

### 3. Publier un Événement (Publish)
Le système source (ex: `InventoryManager` ou `InteractionController`) publie l'événement lorsqu'une action se produit.

```csharp
public class ResourceNode
{
    private readonly IEventBus m_eventBus;

    public void Harvest()
    {
        // ... logique de récolte ...

        m_eventBus.Publish(new ResourceHarvestedEvent("Wood", 5));
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
        m_eventBus.Subscribe<ResourceHarvestedEvent>(OnResourceHarvested);
    }

    public override void _ExitTree()
    {
        m_eventBus.Unsubscribe<ResourceHarvestedEvent>(OnResourceHarvested);
    }
}
```

## Thread-Safety

L'implémentation `EventBus` gère de manière basique la sécurité des threads (Thread-Safety) en utilisant des blocs `lock` autour du dictionnaire des souscriptions (`m_subscriptions`). Lors de la publication (`Publish`), une copie (snapshot) des souscripteurs actifs est créée pour éviter les interblocages (deadlocks) ou les exceptions de collection modifiée si un souscripteur publie un autre événement ou modifie ses souscriptions durant le traitement.
