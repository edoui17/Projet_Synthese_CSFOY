# Guide: Création et Gestion des Tweens dans Godot

Les **Tweens** sont un outil extrêmement puissant et optimisé dans Godot pour animer des propriétés par le code (position, échelle, couleur, rotation, etc.) de manière fluide, sans avoir à configurer de `AnimationPlayer`.

Dans le cadre du jeu **IslandSurvivor**, nous les utilisons abondamment pour le *Game Feel* (Juiciness), par exemple pour l'effet d'impact (Shake) et pour le largage du butin des ressources (Burst + Homing vers le joueur).

Voici comment comprendre, créer et gérer ces Tweens, à la fois via le code C# (puisqu'ils se créent en temps réel) et via ce que l'on peut manipuler.

---

## 1. Créer un Tween en C#

Un Tween dans Godot 4 ne se crée plus en ajoutant un "Node Tween" dans la scène (comme c'était le cas dans Godot 3). Il est créé programmatiquement à la volée.

Pour créer un Tween sur n'importe quel `Node`, appelez simplement :

```csharp
Tween myTween = CreateTween();
// ou sur un noeud spécifique :
Tween myTween = myNode.CreateTween();
```

*Note : Le Tween est automatiquement lié à l'arbre de scènes (SceneTree) et sera détruit de lui-même (Garbage Collected) une fois que toutes ses animations seront terminées.*

---

## 2. Animer une propriété simple (TweenProperty)

La fonction principale que vous utiliserez est `TweenProperty()`.

**Syntaxe :**
`myTween.TweenProperty(ObjetCible, "nom_de_la_propriete_godot", ValeurCible, DureeEnSecondes);`

### Exemple : Déplacer un objet
```csharp
Tween tween = CreateTween();
// Déplace le noeud courant à la position (100, 100) en 1 seconde.
tween.TweenProperty(this, "global_position", new Vector2(100, 100), 1.0f);
```

### Transition (Trans) et Lissage (Ease)
Pour rendre le mouvement moins mécanique et plus "Juicy", on modifie la courbe d'accélération avec `SetTrans()` et `SetEase()`.

- **SetTrans()** : Le type de courbe mathématique (Linéaire, Sine, Quad, Back, Bounce...).
  - *Astuce:* `Tween.TransitionType.Back` est parfait pour donner une sensation d'élasticité ou de recul.
- **SetEase()** : Où s'applique l'effet sur le temps.
  - *In* : Effet au début.
  - *Out* : Effet à la fin (Très utilisé pour ralentir progressivement l'objet avant l'arrêt complet).
  - *InOut* : Effet au début et à la fin.

```csharp
tween.TweenProperty(this, "global_position", new Vector2(100, 100), 1.0f)
     .SetTrans(Tween.TransitionType.Quad)
     .SetEase(Tween.EaseType.Out);
```

---

## 3. Exécution en Séquence ou en Parallèle

Par défaut, si vous ajoutez plusieurs actions à un Tween, elles s'exécutent **les unes à la suite des autres (Séquentiellement)**.

### Mode Séquence (Par défaut)
```csharp
Tween tween = CreateTween();
// 1. Déplacement (0.5s)
tween.TweenProperty(this, "global_position", Vector2.Zero, 0.5f);
// 2. Ensuite, Rotation (0.5s)
tween.TweenProperty(this, "rotation_degrees", 90.0f, 0.5f);
```

### Mode Parallèle
Si vous voulez qu'une animation se joue *en même temps* que la précédente, utilisez `.Parallel()`.

```csharp
Tween tween = CreateTween();
// Le déplacement ET le changement d'échelle se font en même temps !
tween.TweenProperty(this, "global_position", Vector2.Zero, 0.5f);
tween.Parallel().TweenProperty(this, "scale", new Vector2(2, 2), 0.5f);
```

---

## 4. Ajouter des pauses et des délais

Vous pouvez ajouter des temps de pause directement dans la chaîne du Tween en utilisant `TweenInterval()`, ou repousser le lancement d'une propriété spécifique avec `.SetDelay()`.

```csharp
Tween tween = CreateTween();
// Avance
tween.TweenProperty(this, "global_position", new Vector2(10, 0), 0.5f);

// Fait une pause de 1 seconde au sol
tween.TweenInterval(1.0f);

// Puis continue d'avancer
tween.TweenProperty(this, "global_position", new Vector2(20, 0), 0.5f);
```

---

## 5. Exécuter du code à la fin (Finished)

Les Tweens ont un système d'événement natif en C# (`Finished`) très pratique pour supprimer un objet ou déclencher un signal une fois l'animation complétée.

```csharp
Tween tween = CreateTween();
tween.TweenProperty(this, "scale", Vector2.Zero, 0.5f);

// Appelle la méthode "OnAnimationFinished" quand le scale est à 0.
tween.Finished += OnAnimationFinished;

private void OnAnimationFinished()
{
    QueueFree(); // Détruit l'objet
}
```

---

## 6. Cas Pratique Avancé : Le Homing (Traquer une cible mouvante)

Un `TweenProperty` classique prend une position fixe à son lancement. Si la cible (ex: le Joueur) bouge pendant que l'objet vole vers lui, l'objet ratera la cible !

Pour résoudre cela, on utilise **`TweenMethod`** qui au lieu de changer une propriété native Godot, va appeler notre propre fonction C# en lui passant un pourcentage (de 0.0 à 1.0) sur un temps donné.

### Exemple tiré de `ResourceDrop.cs` :

```csharp
private void AnimateToTarget()
{
    Tween moveTween = CreateTween();

    // De 0.0 à 1.0 sur 0.5 secondes, on appelle MoveStep(float p_progress)
    moveTween.TweenMethod(Callable.From<float>(MoveStep), 0.0f, 1.0f, 0.5f)
         .SetTrans(Tween.TransitionType.Back)
         .SetEase(Tween.EaseType.In);

    moveTween.Finished += OnAnimationFinished;
}

private void MoveStep(float p_progress)
{
    // On met à jour la position cible actuelle du Joueur à CHAQUE FRAME de l'animation !
    Vector2 currentTargetPos = m_targetNode.GlobalPosition;

    // On utilise Lerp (Linear Interpolation) pour déplacer fluidement la ressource
    // vers la position actuelle du joueur.
    GlobalPosition = GlobalPosition.Lerp(currentTargetPos, p_progress);
}
```

### Pourquoi c'est Juicy ?
Le fait de réévaluer le point de départ du `Lerp()` à partir de sa propre `GlobalPosition` à chaque fois qu'elle est mise à jour génère une accélération non-linéaire. L'objet démarre doucement, puis semble s'accrocher et foncer comme un aimant vers la toute fin de l'animation, ce qui correspond exactement à l'effet de "Homing" attendu pour la récupération de loot !