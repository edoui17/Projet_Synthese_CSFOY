# SignalManager

## Comprendre le Pattern "Weak Event" (Evenements Faibles)

Étant donné que nous utilisons une architecture C# pure pour la logique, nous rencontrons un defi classique de la programmation orientee objet : le **Lapsed Listener Problem** (le probleme de l'abonne fantome).

### Qu'est-ce qu'une reference forte vs faible ?
*   **Reference Forte (Strong Reference) :** En C# standard, quand vous vous abonnez a un evenement (`OnScoreChanged += MaMethode;`), l'emetteur de l'evenement garde une reference "forte" vers l'objet qui ecoute. Le ramasse-miettes (Garbage Collector) de .NET ne detruira **jamais** cet objet tant que l'emetteur existe et pointe vers lui.
*   **Reference Faible (Weak Reference) :** Une reference faible permet de pointer vers un objet, mais sans interdire au Garbage Collector de le detruire. Si personne d'autre n'utilise l'objet, il sera supprime de la memoire de facon securisee.

### Le probleme dans notre jeu (Godot)
Imaginez que vous ouvrez le Menu de Pause. Ce menu s'abonne au `SignalManager` pour mettre a jour l'affichage. Ensuite, vous fermez le menu, et Godot le detruit (`QueueFree()`).
Cependant, si nous utilisions un evenement C# standard (`event Action`), le `SignalManager` (qui est un Autoload global et ne meurt jamais) garderait une reference forte vers le menu detruit. Resultat : **Fuite de memoire (Memory Leak)**, l'objet reste en memoire C# a l'infini, et le jeu plantera la prochaine fois que le `SignalManager` essaiera d'envoyer un signal au menu detruit.

### La solution : L'EventBus et la classe `WeakAction<T>`
Pour régler cela, le projet évite l'utilisation des événements C# standard (`event Action`) pour la communication globale. À la place, il repose sur l'**EventBus** (voir [EventBus.md](./EventBus.md)), qui utilise une classe personnalisée appelée `WeakAction<T>`.

Cette classe enveloppe le délégué de l'abonné dans une `WeakReference`. Ainsi, quand Godot détruit un élément de l'interface graphique (UI) ou un ennemi, la référence faible dans le `WeakAction<T>` devient nulle. Le Garbage Collector nettoie la mémoire, et l'EventBus détectera automatiquement que l'abonné est mort (`IsAlive == false`) lors du prochain traitement d'événements, le retirant de la liste silencieusement. Cela rend notre système robuste, évite les plantages liés au Lapsed Listener Problem, et libère les développeurs de l'obligation absolue de se désabonner manuellement lors de la destruction des objets (bien que l'appel à `Unsubscribe` reste une bonne pratique, en particulier pour les objets à courte durée de vie).

## Le Rôle de Bridge (Translator) et l'EventBus

Dans l'architecture actuelle, **le `SignalManager` agit strictement comme un Translator (ou Bridge) entre Godot et le Core**. Le Core n'utilise plus les signaux natifs ou les événements C# de manière globale ; il repose entièrement sur l'**EventBus**.

Les responsabilités du `SignalManager` sont donc de deux ordres :
1. **Écouter le Core (EventBus) -> Émettre vers Godot (`[Signal]`)** : Le `SignalManager` s'abonne à certains `IEvent`s via l'EventBus (ex: `ScoreChangedEvent`). Lorsqu'il reçoit cet événement du Core, il émet un signal natif Godot (`EmitSignal(...)`) pour que les interfaces utilisateurs (UI) ou les nœuds visuels puissent réagir sans connaître l'EventBus.
2. **Recevoir de Godot (`[Signal]`) -> Publier vers le Core (EventBus)** : Lorsqu'une interaction purement Godot survient (ex: clic sur un bouton d'interface ou collision), une méthode du `SignalManager` peut être appelée pour créer un `IEvent` (ex: `NavigationRequestedEvent`) et le publier sur l'EventBus, permettant au Core de traiter la demande sans dépendre de Godot.

## Architecture

Le `SignalManager` (qui est un **Autoload** Godot dans `Src/IslandSurvivor/Globals/SignalManager.cs`) fait le pont entre l'architecture `Core` et le moteur Godot en utilisant l'injection de dépendances pour accéder à l'EventBus.

*Note : Les classes `ISignalManager` et `SignalManagerCore` avec leurs `WeakEvents` ont été supprimées de la communication inter-systèmes au profit de ce modèle strict Pub-Sub via l'EventBus.*

## Comment ajouter une nouvelle communication Godot <-> Core

Pour ajouter un nouveau signal global déclenché par le Core à destination de l'interface Godot :

1.  **Créer un événement dans le Core** : Créez un nouvel `IEvent` (ex: `MyCustomEvent.cs`) dans `Src/Core/Events/`.
2.  **Déclarer le Signal natif dans Godot** : Dans `SignalManager.cs`, ajoutez l'attribut `[Signal]` et son délégué (ex: `[Signal] public delegate void MyCustomEventHandler();`).
3.  **Abonnement dans le `_Ready`** : Dans le `_Ready` du `SignalManager.cs`, abonnez-vous à l'événement de l'EventBus : `m_eventBus.Subscribe<MyCustomEvent>(OnMyCustomEvent)`.
4.  **Redéclencher le Signal** : Dans le callback (`OnMyCustomEvent`), émettez le signal natif : `EmitSignal(SignalName.MyCustomEvent)`.

### Exemple complet : Mise à jour du Score

**Dans le Core (ScoreTracker.cs publie un événement) :**
```csharp
// Le Core publie simplement l'événement
m_eventBus.Publish(new ScoreChangedEvent(oldScore, newScore));
```

**Dans le SignalManager (Bridge en Godot) :**
```csharp
public partial class SignalManager : Node
{
    // 1. Définition du signal natif pour les autres noeuds Godot
    [Signal]
    public delegate void ScoreChangedEventHandler(int p_previousScore, int p_newScore);

    public override void _Ready()
    {
        // 2. Abonnement à l'EventBus du Core
        m_eventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
    }

    private void OnScoreChanged(ScoreChangedEvent p_event)
    {
        // 3. Traduction en signal natif Godot
        EmitSignal(SignalName.ScoreChanged, p_event.PreviousScore, p_event.NewScore);
    }
}
```

**Dans un noeud UI (ex: ScoreLabel.cs) :**
```csharp
public override void _Ready()
{
    // L'UI s'abonne au signal natif Godot de l'Autoload, ignorant totalement l'EventBus
    SignalManager.Instance.ScoreChanged += OnScoreChangedHandler;
}

private void OnScoreChangedHandler(int p_previousScore, int p_newScore)
{
    Text = $"Score: {p_newScore}";
}

public override void _ExitTree()
{
    // Toujours se désabonner des signaux natifs pour éviter les fuites ou erreurs de Godot
    if (SignalManager.Instance != null)
    {
        SignalManager.Instance.ScoreChanged -= OnScoreChangedHandler;
    }
}
```

*Règle d'or : La couche `/Src/Core` doit strictement utiliser l'EventBus et ses `IEvent`s. Elle ne doit jamais utiliser de signaux Godot. La couche UI (`/Src/IslandSurvivor`) doit écouter les signaux natifs traduits par le `SignalManager` ou s'abonner occasionnellement à l'EventBus si elle a besoin d'interagir directement avec la logique métier.*