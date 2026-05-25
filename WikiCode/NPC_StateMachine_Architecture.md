# Architecture du State Machine pour les PNJ

Cette documentation décrit la nouvelle architecture basée sur des nœuds (Node-based) utilisée pour la logique comportementale de tous les PNJ (Passifs, Agressifs, et Boss).

## Concept Principal

Plutôt que d'utiliser des blocs `if/else` complexes directement dans la méthode `_PhysicsProcess` des classes C# (comme l'ancien système `AgressorController`), nous utilisons désormais un modèle de conception d'état par composition de nœuds (Composition Pattern).

Chaque comportement (ex: chasser, attaquer, fuir) est un nœud enfant (`State`) attaché à un nœud parent gérant la transition (`StateMachine`).

## Hiérarchie des Nœuds

Pour fonctionner correctement, un PNJ doit comporter la structure suivante dans sa scène `.tscn` :

```text
NpcBase (CharacterBody2D)
├── Sprite2D
├── AnimationPlayer
├── MovementController
└── StateMachine (Node)
    ├── IdleState (Node)
    ├── ChaseState (Node)
    ├── AttackState (Node)
    └── DeathState (Node)
```

## Classes Principales

### `StateMachine`
Le cerveau du système. Il garde une référence vers le `CurrentState` et redirige les appels de la boucle Godot (`_Process` et `_PhysicsProcess`) vers l'état actif. Il écoute le signal `TransitionRequested` émis par les états pour changer de comportement.

### `State`
La classe de base pour tous les comportements. Les états concrets héritent de cette classe et peuvent surcharger les méthodes suivantes :
- `Enter()` : Appelé au moment où l'état devient actif.
- `Exit()` : Appelé juste avant que l'état ne soit quitté.
- `Update(double p_delta)` : Exécuté à chaque frame.
- `PhysicsUpdate(double p_delta)` : Exécuté à chaque frame physique.

### États Concrets Réutilisables
Toutes les variables de configuration de ces états (vitesse, distance, nom d'animation) sont exposées dans l'Inspecteur Godot via l'attribut `[Export]`.

- `IdleState` : Le PNJ attend ou patrouille. Il passe en `ChaseState` s'il détecte le joueur.
- `ChaseState` : Le PNJ se déplace vers sa cible via le `MovementController`.
- `AttackState` : Demande au `AttackController` d'exécuter l'attaque. Écoute la fin de l'animation pour retourner au `ChaseState`.
- `FleeState` : Utilisé principalement par les `PassiveNpcBase`. Le PNJ s'enfuit à l'opposé de la source de dégâts.
- `DeathState` : Stoppe tous les mouvements et joue l'animation de mort.
- `WindUpState`, `DashState`, `RecoveryState` : États avancés utilisés par les ennemis comme le `Lancer` pour effectuer des attaques chargées et des esquives.

## Gestion des Animations

Le système ne dépend plus du nœud `AnimatedSprite2D`. Il utilise exclusivement un `Sprite2D` standard couplé à un `AnimationPlayer`.

Chaque nœud `State` possède une propriété `AnimationName` et `FallbackAnimationName` exposées dans l'Inspecteur. Lors de l'appel à `Enter()`, l'état vérifie si l'animation existe (via `m_animationPlayer.HasAnimation()`) et la joue. Si elle n'existe pas, il tente de jouer la `FallbackAnimationName` pour éviter que le jeu ne plante.

## Orientation Visuelle (`FlipH`)

La logique d'orientation du sprite n'est plus gérée dans les états. C'est désormais le `MovementController` qui est responsable de basculer la propriété `FlipH` du `Sprite2D` en fonction du vecteur de mouvement.
