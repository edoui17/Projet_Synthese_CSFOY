# Guide d'Utilisation : ScoreManager & N-Tier Architecture

## Architecture Globale

Le système de score a été conçu en respectant l'architecture N-Tier du projet, assurant une séparation stricte entre la logique métier (Core) et l'affichage (Godot).

1.  **Core (`SessionState`, `ScoreTracker`)** : Gère l'état vivant (Live State). Il valide les points, accumule le score et gère la logique de record (High Score).
2.  **Godot Resources (`SessionResource`)** : Agit comme un template immuable. Il fournit les valeurs de départ lors de l'initialisation.
3.  **Bridge (`ScoreManager` / `SignalManager`)** : Ce sont les nœuds Godot qui font le lien. Ils écoutent les événements du Core (`ScoreChangedEvent` via l'**EventBus**) et les transforment en signaux Godot (`[Signal]`).
4.  **Save System (`ISaveService`, `GodotSaveService`)** : Le Core utilise une interface pour sauvegarder les données, ce qui permet à Godot d'injecter sa propre implémentation utilisant `FileAccess`.

---

## 1. Configuration Initiale (Godot)

### A. Créer un Template de Session (Resource)
1. Dans l'éditeur Godot, créez une nouvelle ressource de type `SessionResource`.
2. Configurez les valeurs de départ : `StartingScore`, `StartingMapCount`, `StartingCharacterId`.
3. Sauvegardez cette ressource dans `Src/IslandSurvivor/Resources/Stats/` (ex: `DefaultSession.tres`).

### B. Mettre en place le ScoreManager
1. Ajoutez le nœud `ScoreManager.cs` dans votre scène principale ou votre `GameManager`.
2. Dans l'inspecteur, assignez le template `SessionResource` que vous venez de créer.
3. Le `ScoreManager` s'occupera automatiquement de récupérer l'instance du `ScoreTracker` du Core via l'injection de dépendances (`ServiceRegistry.Instance.ScoreTracker`).

---

## 2. Utilisation dans le Jeu (Côté Godot)

Pour interagir avec le score depuis vos scripts Godot, vous devez toujours passer par le `ScoreManager`.

### Ajouter des Points
Lorsqu'un joueur effectue une action gratifiante :

```csharp
// Exemple dans un script de ramassage ou d'ennemi vaincu
ScoreManager scoreManager = GetNode<ScoreManager>("/root/ScoreManager"); // Ou via une référence Export
scoreManager.AddScore(50); // Ajoute 50 points (les valeurs négatives sont ignorées par le Core)
```

### Récupérer les Valeurs Actuelles

```csharp
int currentScore = scoreManager.GetCurrentScore();
int currentHighScore = scoreManager.GetHighScore();
```

### Mettre à Jour le High Score
Généralement appelé à la fin d'une partie ou d'une session :

```csharp
scoreManager.UpdateHighScore(); // Le Core vérifiera si le score actuel dépasse le record et sauvegardera si nécessaire
```

---

## 3. Mise à Jour de l'UI (Signaux)

Pour mettre à jour l'interface utilisateur, connectez-vous au signal émis par le `ScoreManager`. Le Bridge pattern assure que ce signal est déclenché par le Core.

### Exemple de Script UI (`ScoreUI.cs`)

```csharp
using Godot;
using IslandSurvivor.Nodes.StatsManager;

public partial class ScoreUI : Control
{
    [Export] private ScoreManager m_scoreManager;
    [Export] private Label m_scoreLabel;

    public override void _Ready()
    {
        if (m_scoreManager != null)
        {
            // S'abonner au signal
            m_scoreManager.ScoreChanged += OnScoreChanged;
        }
    }

    private void OnScoreChanged(int p_previousScore, int p_newScore)
    {
        // Mettre à jour le texte à l'écran
        m_scoreLabel.Text = $"Score: {p_newScore}";
        
        // Optionnel : Ajouter une animation visuelle ici
    }

    public override void _ExitTree()
    {
        // Toujours se désabonner pour éviter les fuites
        if (m_scoreManager != null)
        {
            m_scoreManager.ScoreChanged -= OnScoreChanged;
        }
    }
}
```

## Résumé du Flux de Données

1. Action Godot -> `ScoreManager.AddScore(X)`
2. `ScoreManager` -> `ScoreTracker.AddScore(X)` (Core)
3. `ScoreTracker` valide l'action et modifie `SessionState`
4. `ScoreTracker` publie un événement `ScoreChangedEvent` via l'**EventBus**
5. Le Bridge (ex: `SignalManager` ou `ScoreManager` en Godot) reçoit l'événement de l'EventBus et émet le signal natif Godot `ScoreChanged`
6. `ScoreUI` reçoit le signal Godot et met à jour l'affichage.