# SignalManager

## Architecture
Le `SignalManager` sert de point de communication global (Observer Pattern) pour tous les systèmes du jeu. Il est conçu pour respecter l'architecture N-Tier du projet :

- **Core (`Src/Core/Interfaces/ISignalManager.cs`)** : Définit les contrats (Interfaces) que tout gestionnaire de signaux doit implémenter.
- **Utils (`Src/Core/Utils/WeakEvent.cs` et `Src/Core/Utils/WeakEventNonGeneric.cs`)** : Implémente le pattern **Weak Event** avec `WeakReference`. Cela garantit que si une scène est détruite dans Godot et qu'un `Node` n'est plus en mémoire, le système de signaux ne causera pas de fuite de mémoire ou de plantage (car le *Garbage Collector* pourra détruire le nœud abonné). Le pattern est séparé en deux fichiers pour la version générique (`<TEventArgs>`) et non générique.
- **Core Implementation (`Src/Core/Managers/SignalManagerCore.cs`)** : Gère la logique pure C# de l'envoi de signaux.
- **Godot (`Src/IslandSurvivor/Globals/SignalManager.cs`)** : Fait le pont entre l'architecture `Core` et le moteur Godot. Défini en tant qu'**Autoload**, il expose la logique `Core` via délégation.

## Conventions de code appliquées

Afin de respecter les standards stricts du projet, le code implémenté suit ces règles :
- **Typage explicite** : Le mot-clé `var` n'est **jamais** utilisé.
- **Ordre des membres** : Toutes les données membres (champs `m_`) sont déclarées tout en haut de chaque classe.
- **Ordre des propriétés** : Toutes les propriétés publiques/privées sont déclarées juste en dessous des données membres, avant les constructeurs et méthodes.
- **Séparation par fichier** : Chaque classe (même utilitaire) est isolée dans son propre fichier (ex: `WeakEvent.cs` et `WeakEventNonGeneric.cs`).
- **Interfaces abstraites** : L'interface `ISignalManager` et ses implémentations ne contiennent pas de signaux prédéfinis pour le moment, seulement des exemples en commentaire pour guider les futurs développements.

## Comment ajouter un signal

Pour ajouter un nouveau signal global, suivez ces étapes (en gardant les conventions ci-dessus à l'esprit) :

1.  **Dans `ISignalManager.cs`** : Déclarez votre type `EventArgs` (dans son propre fichier si public) et ajoutez la propriété `WeakEvent` ainsi que la méthode `Emit` associée dans l'interface.
2.  **Dans `SignalManagerCore.cs`** : Implémentez l'instanciation de l'événement (donnée membre en haut) et sa méthode `Emit`.
3.  **Dans `SignalManager.cs` (Autoload)** : Déléguez les appels vers la classe `SignalManagerCore`.

### Exemple

**Déclaration dans le Core (ISignalManager.cs) :**
```csharp
public class ScoreChangedEventArgs : EventArgs
{
    public int NewScore { get; }
    public ScoreChangedEventArgs(int p_newScore) => NewScore = p_newScore;
}

WeakEvent<ScoreChangedEventArgs> OnScoreChanged { get; }
void EmitScoreChanged(object p_sender, int p_newScore);
```

**Implémentation dans le Client Godot :**
```csharp
// S'abonner à un événement (Dans un Node, par ex. _Ready)
SignalManager.Instance.OnScoreChanged.AddListener(OnScoreChangedHandler);

// Émettre un événement
SignalManager.Instance.EmitScoreChanged(this, 1500);

// Méthode de réception
private void OnScoreChangedHandler(object p_sender, ISignalManager.ScoreChangedEventArgs p_args)
{
    // Mettre à jour l'UI avec p_args.NewScore
}
```

*Note : Bien que `WeakEvent` utilise des références faibles, il est toujours recommandé de se désabonner (`RemoveListener`) lors de la destruction d'un objet (`_ExitTree`) pour des raisons de performance et de bonnes pratiques.*