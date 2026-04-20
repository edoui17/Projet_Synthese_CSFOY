# SignalManager

## Comprendre le Pattern "Weak Event" (Evenements Faibles)

Étant donné que nous utilisons une architecture C# pure pour la logique, nous rencontrons un defi classique de la programmation orientee objet : le **Lapsed Listener Problem** (le probleme de l'abonne fantome).

### Qu'est-ce qu'une reference forte vs faible ?
*   **Reference Forte (Strong Reference) :** En C# standard, quand vous vous abonnez a un evenement (`OnScoreChanged += MaMethode;`), l'emetteur de l'evenement garde une reference "forte" vers l'objet qui ecoute. Le ramasse-miettes (Garbage Collector) de .NET ne detruira **jamais** cet objet tant que l'emetteur existe et pointe vers lui.
*   **Reference Faible (Weak Reference) :** Une reference faible permet de pointer vers un objet, mais sans interdire au Garbage Collector de le detruire. Si personne d'autre n'utilise l'objet, il sera supprime de la memoire de facon securisee.

### Le probleme dans notre jeu (Godot)
Imaginez que vous ouvrez le Menu de Pause. Ce menu s'abonne au `SignalManager` pour mettre a jour l'affichage. Ensuite, vous fermez le menu, et Godot le detruit (`QueueFree()`).
Cependant, si nous utilisions un evenement C# standard (`event Action`), le `SignalManager` (qui est un Autoload global et ne meurt jamais) garderait une reference forte vers le menu detruit. Resultat : **Fuite de memoire (Memory Leak)**, l'objet reste en memoire C# a l'infini, et le jeu plantera la prochaine fois que le `SignalManager` essaiera d'envoyer un signal au menu detruit.

### La solution : Nos classes `WeakEvent` et `WeakEvent<TEventArgs>`
Pour regler cela, nous n'utilisons pas le mot-cle `event` de C#. Nous avons cree nos propres classes : `WeakEvent` (pour les evenements sans arguments) et `WeakEvent<TEventArgs>` (pour les evenements avec des donnees, comme le nouveau score).

Ces classes enveloppent l'abonne dans une `WeakReference`. Ainsi, quand Godot detruit un element de l'interface graphique (UI) ou un ennemi, la reference faible dans le `WeakEvent` devient nulle. Le Garbage Collector nettoie la memoire, et le `SignalManager` detectera automatiquement que l'abonne est mort (`IsAlive == false`) et le retirera de la liste silencieusement. Cela rend notre systeme robuste, evite les plantages, et libere les developpeurs de l'obligation absolue de se desabonner manuellement lors de la destruction des objets (bien que cela reste une bonne pratique).

## Architecture
Le `SignalManager` sert de point de communication global (Observer Pattern) pour tous les systèmes du jeu. Il est conçu pour respecter l'architecture N-Tier du projet :

- **Core (`Src/Core/Interfaces/ISignalManager.cs`)** : Définit les contrats (Interfaces) que tout gestionnaire de signaux doit implémenter.
- **Utils (`Src/Core/Utils/WeakEvent.cs` et `Src/Core/Utils/WeakEventNonGeneric.cs`)** : Implémente le pattern **Weak Event** avec `WeakReference`. Cela garantit que si une scène est détruite dans Godot et qu'un `Node` n'est plus en mémoire, le système de signaux ne causera pas de fuite de mémoire ou de plantage (car le *Garbage Collector* pourra détruire le nœud abonné). Le pattern est séparé en deux fichiers pour la version générique (`<TEventArgs>`) et non générique.
- **Core Implementation (`Src/Core/Managers/SignalManagerCore.cs`)** : Gère la logique pure C# de l'envoi de signaux.
- **Godot (`Src/IslandSurvivor/Globals/SignalManager.cs`)** : Fait le pont entre l'architecture `Core` et le moteur Godot. Défini en tant qu'**Autoload**, il expose la logique `Core` via délégation.

## Comment ajouter un signal

Pour ajouter un nouveau signal global, suivez ces étapes :

1.  **Dans `ISignalManager.cs`** : Déclarez votre type `EventArgs` (dans son propre fichier si public) et ajoutez la propriété `WeakEvent` ainsi que la méthode `Emit` associée dans l'interface.
2.  **Dans `SignalManagerCore.cs`** : Implémentez l'instanciation de l'événement (donnée membre en haut) et sa méthode `Emit`.
3.  **Dans `SignalManager.cs` (Autoload)** : Déléguez les appels vers la classe `SignalManagerCore`.

### Exemple

**Déclaration dans le Core (ISignalManager.cs) :**
```csharp
public class ScoreChangedEventArgs : EventArgs
{
    public ScoreChangedEventArgs(int p_newScore) => NewScore = p_newScore;

    public int NewScore { get; }
}

WeakEvent<ScoreChangedEventArgs> OnScoreChanged { get; }
void EmitScoreChanged(object p_sender, int p_newScore);
```

**Implémentation dans le Client Godot :**
```csharp
// S'abonner au signal natif Godot (Bridge)
SignalManager.Instance.ScoreChanged += OnScoreChangedHandler;

// Émettre un événement (Il passera par le Core avant de revenir en Godot)
SignalManager.Instance.EmitScoreChanged(this, 1500);

// Méthode de réception
private void OnScoreChangedHandler(int p_previousScore, int p_newScore)
{
    // Mettre à jour l'UI avec p_newScore
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

*Note sur le Bridge : Le Godot Client ne doit jamais utiliser `.AddListener` pour écouter des événements, seulement `+=` sur les signaux natifs (`[Signal]`). Les `WeakEvent` avec `.AddListener` sont réservés exclusivement à la logique interne du Core.*